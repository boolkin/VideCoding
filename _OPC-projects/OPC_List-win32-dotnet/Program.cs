using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.IO;
using Opc;
using Opc.Da;
using OpcCom;

namespace OpcApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }

    // --- ВСПОМОГАТЕЛЬНЫЙ КЛАСС: МАСТЕР ТЕГА ---
    public class TagEditorDialog : Form
    {
        public string K { get; private set; } = "1";
        public string B { get; private set; } = "0";
        public string SelectedType { get; private set; } = "s";
        private TextBox txtK = new() { Text = "1", Dock = DockStyle.Top };
        private TextBox txtB = new() { Text = "0", Dock = DockStyle.Top };
        private ComboBox cbType = new() { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };

        public TagEditorDialog(string tagName, string k = "1", string b = "0", string type = "s")
        {
            this.Text = "Параметры тега: " + tagName;
            this.Size = new Size(300, 250);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;

            cbType.Items.AddRange(new object[] { 
                "s - Аналог (V*K+B)", 
                "b - Булево (ВКЛ/ВЫКЛ)", 
                "!b - Инверсия (ВЫКЛ/ВКЛ)", 
                "t - Текст" 
            });

            txtK.Text = k; txtB.Text = b;
            int idx = type == "s" ? 0 : type == "b" ? 1 : type == "!b" ? 2 : 3;
            cbType.SelectedIndex = idx;

            Button btnOk = new() { Text = "СОХРАНИТЬ", Dock = DockStyle.Bottom, Height = 40, BackColor = Color.LightGreen };
            btnOk.Click += (s, e) => {
                K = txtK.Text; B = txtB.Text;
                SelectedType = cbType.SelectedIndex == 0 ? "s" : cbType.SelectedIndex == 1 ? "b" : cbType.SelectedIndex == 2 ? "!b" : "t";
                this.DialogResult = DialogResult.OK;
            };

            this.Controls.Add(btnOk);
            this.Controls.Add(cbType); this.Controls.Add(new Label { Text = "Тип данных:", Dock = DockStyle.Top });
            this.Controls.Add(txtB); this.Controls.Add(new Label { Text = "Смещение B:", Dock = DockStyle.Top });
            this.Controls.Add(txtK); this.Controls.Add(new Label { Text = "Коэффициент K:", Dock = DockStyle.Top });
            this.Padding = new Padding(10);
        }
    }

    public class MainForm : Form
    {
        private ListBox lbServers = new() { Dock = DockStyle.Fill, HorizontalScrollbar = true };
        private ListBox lbTags = new() { Dock = DockStyle.Fill, HorizontalScrollbar = true };
        private ListView lvFavorites = new() { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true };
        private TextBox txtSearch = new() { Dock = DockStyle.Top, PlaceholderText = "Поиск тега..." };
        private GroupBox gbTags;
        private List<string> fullTagList = new();
        private System.Windows.Forms.Timer autoUpdateTimer = new();
        private Opc.Da.Server? selectedServer;
        private Opc.Da.Subscription? monitoringGroup;
        private string defaultFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tags.txt");

        private Button btnConnect = new() { Text = "ПОДКЛЮЧИТЬСЯ", Height = 30, Width = 150, BackColor = Color.LightBlue };
        private Button btnMonitor = new() { Text = "СТАРТ МОНИТОРИНГ", Height = 40, Dock = DockStyle.Bottom, BackColor = Color.LightGreen, Font = new Font("Segoe UI", 9, FontStyle.Bold) };

        public MainForm()
        {
            this.Text = "OPC DA Tag Master";
            this.Size = new Size(1100, 800);

            lvFavorites.Columns.Add("#", 40);
            lvFavorites.Columns.Add("Тег (System Name)", 300);
            lvFavorites.Columns.Add("Значение", 150);
            lvFavorites.Columns.Add("K", 60);
            lvFavorites.Columns.Add("B", 60);
            lvFavorites.Columns.Add("Тип", 60);

            autoUpdateTimer.Interval = 1000;
            autoUpdateTimer.Tick += (s, e) => ReadValues();

            FlowLayoutPanel filePanel = new() { Dock = DockStyle.Top, Height = 45, BackColor = Color.LightGray, Padding = new Padding(5) };
            Button btnSave = new() { Text = "💾 Сохранить", AutoSize = true };
            Button btnLoad = new() { Text = "📂 Загрузить", AutoSize = true };
            btnSave.Click += (s, e) => SaveProject(null); // Ручное сохранение
            btnLoad.Click += (s, e) => LoadProject(null);

            filePanel.Controls.AddRange(new Control[] { btnConnect, new Label { Width = 20 }, btnSave, btnLoad });

            TableLayoutPanel rootLayout = new() { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            GroupBox gbServers = new GroupBox { Text = "1. ВЫБОР СЕРВЕРА", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 8, FontStyle.Bold) };
            gbServers.Controls.Add(lbServers);

            TableLayoutPanel bottomLayout = new() { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));

            gbTags = new GroupBox { Text = "2. ТЕГИ", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 8, FontStyle.Bold) };
            gbTags.Controls.Add(lbTags); gbTags.Controls.Add(txtSearch);

            Panel favPanel = new() { Dock = DockStyle.Fill };
            favPanel.Controls.Add(lvFavorites);
            favPanel.Controls.Add(btnMonitor);

            bottomLayout.Controls.Add(gbTags, 0, 0);
            bottomLayout.Controls.Add(favPanel, 1, 0);

            rootLayout.Controls.Add(filePanel, 0, 0);
            rootLayout.Controls.Add(gbServers, 0, 1);
            rootLayout.Controls.Add(bottomLayout, 0, 2);
            this.Controls.Add(rootLayout);

            this.Load += (s, e) => { ScanServers(); AutoLoadDefault(); };
            btnConnect.Click += (s, e) => ConnectAndBrowse();
            lbTags.MouseDoubleClick += (s, e) => AddToFavorites();
            txtSearch.TextChanged += (s, e) => FilterTags();
            lvFavorites.MouseDoubleClick += (s, e) => EditFavoriteParams();
            btnMonitor.Click += (s, e) => { if (autoUpdateTimer.Enabled) StopMonitoring(); else StartMonitoring(); };

            ContextMenuStrip menuFav = new();
            menuFav.Items.Add("Удалить тег", null, (s, e) => RemoveFromFavorites());
            lvFavorites.ContextMenuStrip = menuFav;
        }

        private void AddToFavorites()
        {
            if (lbTags.SelectedItem == null) return;
            string tag = lbTags.SelectedItem.ToString();
            using (var diag = new TagEditorDialog(tag))
            {
                if (diag.ShowDialog() == DialogResult.OK)
                {
                    ListViewItem lvi = new(lvFavorites.Items.Count.ToString());
                    lvi.SubItems.AddRange(new[] { tag, "---", diag.K, diag.B, diag.SelectedType });
                    lvFavorites.Items.Add(lvi);
                    monitoringGroup?.AddItems(new[] { new Item { ItemName = tag } });
                    // SaveProject(defaultFile); // УДАЛЕНО: Автосохранение отключено
                }
            }
        }

        private void EditFavoriteParams()
        {
            if (lvFavorites.SelectedItems.Count == 0) return;
            var row = lvFavorites.SelectedItems[0];
            using (var diag = new TagEditorDialog(row.SubItems[1].Text, row.SubItems[3].Text, row.SubItems[4].Text, row.SubItems[5].Text))
            {
                if (diag.ShowDialog() == DialogResult.OK)
                {
                    row.SubItems[3].Text = diag.K;
                    row.SubItems[4].Text = diag.B;
                    row.SubItems[5].Text = diag.SelectedType;
                    // SaveProject(defaultFile); // УДАЛЕНО: Автосохранение отключено
                }
            }
        }

        private void RemoveFromFavorites()
        {
            if (lvFavorites.SelectedItems.Count > 0)
            {
                lvFavorites.Items.Remove(lvFavorites.SelectedItems[0]);
                // SaveProject(defaultFile); // УДАЛЕНО: Автосохранение отключено
            }
        }

        private void SaveProject(string? path)
        {
            string fn = path;
            if (fn == null)
            {
                using (SaveFileDialog sfd = new() { Filter = "Text Files|*.txt", FileName = "tags.txt" })
                {
                    if (sfd.ShowDialog() == DialogResult.OK) fn = sfd.FileName;
                    else return;
                }
            }

            // Формируем список строк без пустой строки в конце
            var lines = lvFavorites.Items.Cast<ListViewItem>()
                .Select(it => $"{it.Text}\t{it.SubItems[1].Text}\t{it.SubItems[3].Text}\t{it.SubItems[4].Text}\t{it.SubItems[5].Text}")
                .ToArray();

            File.WriteAllLines(fn, lines); // Этот метод не добавляет лишнюю строку в конце
            MessageBox.Show("Проект сохранен!", "Инфо", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadProject(string? path)
        {
            string fn = path;
            if (fn == null)
            {
                using (OpenFileDialog ofd = new() { Filter = "Text Files|*.txt" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK) fn = ofd.FileName;
                    else return;
                }
            }

            if (!File.Exists(fn)) return;
            lvFavorites.Items.Clear();
            foreach (string line in File.ReadAllLines(fn))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] p = line.Split('\t');
                if (p.Length >= 5)
                {
                    ListViewItem lvi = new(p[0]);
                    lvi.SubItems.AddRange(new[] { p[1], "---", p[2], p[3], p[4] });
                    lvFavorites.Items.Add(lvi);
                    monitoringGroup?.AddItems(new[] { new Item { ItemName = p[1] } });
                }
            }
        }

        private void ReadValues()
        {
            if (selectedServer == null || monitoringGroup == null || monitoringGroup.Items.Length == 0) return;
            try
            {
                ItemValueResult[] results = selectedServer.Read(monitoringGroup.Items);
                for (int i = 0; i < results.Length; i++)
                {
                    string rawVal = results[i].Value?.ToString() ?? "";
                    string name = monitoringGroup.Items[i].ItemName;
                    foreach (ListViewItem it in lvFavorites.Items)
                    {
                        if (it.SubItems[1].Text == name)
                        {
                            it.SubItems[2].Text = ProcessValue(rawVal, it.SubItems[5].Text, it.SubItems[3].Text, it.SubItems[4].Text);
                            break;
                        }
                    }
                }
            } catch { }
        }

        private string ProcessValue(string raw, string type, string ks, string bs)
        {
            if (string.IsNullOrEmpty(raw)) return "---";
            if (type == "t") return raw.Trim();
            if (type.Contains("b"))
            {
                bool val = (raw == "1" || raw.ToLower() == "true");
                if (type.StartsWith("!")) val = !val;
                return val ? "ВКЛ" : "ВЫКЛ";
            }
            if (type == "s")
            {
                if (double.TryParse(raw.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double v) &&
                    double.TryParse(ks.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double k) &&
                    double.TryParse(bs.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double b))
                {
                    return (v * k + b).ToString("F2");
                }
            }
            return raw;
        }

        private void AutoLoadDefault() { if (File.Exists(defaultFile)) LoadProject(defaultFile); }

        private void ConnectAndBrowse()
        {
            if (lbServers.SelectedItem is not Opc.Server info) return;
            try {
                if (selectedServer != null) selectedServer.Disconnect();
                selectedServer = new Opc.Da.Server(new OpcCom.Factory(), null);
                selectedServer.Connect(info.Url, new ConnectData(new System.Net.NetworkCredential()));
                monitoringGroup = (Opc.Da.Subscription)selectedServer.CreateSubscription(new SubscriptionState { Name = "HmiGroup", Active = true });
                lbTags.Items.Clear(); fullTagList.Clear();
                BrowseRecursive(null); UpdateTagCounter();
                foreach (ListViewItem it in lvFavorites.Items)
                    monitoringGroup.AddItems(new[] { new Item { ItemName = it.SubItems[1].Text } });
                btnConnect.BackColor = Color.LightGreen; btnConnect.Text = "ПОДКЛЮЧЕНО";
            } catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void ScanServers()
        {
            var s = new OpcCom.ServerEnumerator().GetAvailableServers(Specification.COM_DA_20);
            if (s != null) { lbServers.Items.AddRange(s); lbServers.DisplayMember = "Name"; }
        }

        private void BrowseRecursive(ItemIdentifier? p)
        {
            var e = selectedServer?.Browse(p, new BrowseFilters { BrowseFilter = browseFilter.all }, out _);
            if (e == null) return;
            foreach (var el in e) {
                if (el.IsItem) { lbTags.Items.Add(el.ItemName); fullTagList.Add(el.ItemName); }
                if (el.HasChildren) BrowseRecursive(new ItemIdentifier(el.ItemPath, el.ItemName));
            }
        }

        private void FilterTags()
        {
            lbTags.Items.Clear();
            lbTags.Items.AddRange(fullTagList.Where(t => t.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase)).ToArray());
            UpdateTagCounter();
        }

        private void UpdateTagCounter() => gbTags.Text = $"2. ТЕГИ ({lbTags.Items.Count}/{fullTagList.Count})";

        private void StartMonitoring() { autoUpdateTimer.Start(); btnMonitor.Text = "СТОП"; btnMonitor.BackColor = Color.MistyRose; }
        private void StopMonitoring() { autoUpdateTimer.Stop(); btnMonitor.Text = "СТАРТ МОНИТОРИНГ"; btnMonitor.BackColor = Color.LightGreen; }
    }
}
