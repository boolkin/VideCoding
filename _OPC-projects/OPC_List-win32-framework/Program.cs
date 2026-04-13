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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    // --- ВСПОМОГАТЕЛЬНЫЙ КЛАСС: МАСТЕР ТЕГА ---
    public class TagEditorDialog : Form
    {
        public string K { get; private set; }
        public string B { get; private set; }
        public string SelectedType { get; private set; }

        private TextBox txtK = new TextBox();
        private TextBox txtB = new TextBox();
        private ComboBox cbType = new ComboBox();

        public TagEditorDialog(string tagName, string k = "1", string b = "0", string type = "s")
        {
            this.K = "1";
            this.B = "0";
            this.SelectedType = "s";

            this.Text = "Параметры тега: " + tagName;
            this.Size = new Size(300, 250);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;

            txtK.Text = "1"; txtK.Dock = DockStyle.Top;
            txtB.Text = "0"; txtB.Dock = DockStyle.Top;
            cbType.Dock = DockStyle.Top; 
            cbType.DropDownStyle = ComboBoxStyle.DropDownList;

            cbType.Items.AddRange(new object[] { 
                "s - Аналог (V*K+B)", 
                "b - Булево (ВКЛ/ВЫКЛ)", 
                "!b - Инверсия (ВЫКЛ/ВКЛ)", 
                "t - Текст" 
            });

            txtK.Text = k; txtB.Text = b;
            int idx = type == "s" ? 0 : type == "b" ? 1 : type == "!b" ? 2 : 3;
            cbType.SelectedIndex = idx;

            Button btnOk = new Button();
            btnOk.Text = "СОХРАНИТЬ";
            btnOk.Dock = DockStyle.Bottom;
            btnOk.Height = 40;
            btnOk.BackColor = Color.LightGreen;

            btnOk.Click += (s, e) => {
                K = txtK.Text; B = txtB.Text;
                SelectedType = cbType.SelectedIndex == 0 ? "s" : 
                               cbType.SelectedIndex == 1 ? "b" : 
                               cbType.SelectedIndex == 2 ? "!b" : "t";
                this.DialogResult = DialogResult.OK;
            };

            this.Controls.Add(btnOk);
            this.Controls.Add(cbType); 
            this.Controls.Add(new Label { Text = "Тип данных:", Dock = DockStyle.Top });
            this.Controls.Add(txtB); 
            this.Controls.Add(new Label { Text = "Смещение B:", Dock = DockStyle.Top });
            this.Controls.Add(txtK); 
            this.Controls.Add(new Label { Text = "Коэффициент K:", Dock = DockStyle.Top });
            this.Padding = new Padding(10);
        }
    }

    public class MainForm : Form
    {
        private ListBox lbServers = new ListBox();
        private ListBox lbTags = new ListBox();
        private ListView lvFavorites = new ListView();
        private TextBox txtSearch = new TextBox();
        private GroupBox gbTags;
        private List<string> fullTagList = new List<string>();
        private System.Windows.Forms.Timer autoUpdateTimer = new System.Windows.Forms.Timer();
        private Opc.Da.Server selectedServer;
        private Opc.Da.Subscription monitoringGroup;
        private string defaultFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tags.txt");

        private Button btnConnect = new Button();
        private Button btnMonitor = new Button();

        public MainForm()
        {
            this.Text = "OPC DA Tag Master";
            this.Size = new Size(1100, 800);

            lbServers.Dock = DockStyle.Fill; lbServers.HorizontalScrollbar = true;
            lbTags.Dock = DockStyle.Fill; lbTags.HorizontalScrollbar = true;
            lvFavorites.Dock = DockStyle.Fill; lvFavorites.View = View.Details;
            lvFavorites.FullRowSelect = true; lvFavorites.GridLines = true;
            txtSearch.Dock = DockStyle.Top;

            btnConnect.Text = "ПОДКЛЮЧИТЬСЯ"; btnConnect.Height = 30; btnConnect.Width = 150; btnConnect.BackColor = Color.LightBlue;
            btnMonitor.Text = "СТАРТ МОНИТОРИНГ"; btnMonitor.Height = 40; btnMonitor.Dock = DockStyle.Bottom;
            btnMonitor.BackColor = Color.LightGreen; btnMonitor.Font = new Font("Segoe UI", 9, FontStyle.Bold);

            lvFavorites.Columns.Add("#", 40);
            lvFavorites.Columns.Add("Тег (System Name)", 300);
            lvFavorites.Columns.Add("Значение", 150);
            lvFavorites.Columns.Add("K", 60);
            lvFavorites.Columns.Add("B", 60);
            lvFavorites.Columns.Add("Тип", 60);

            autoUpdateTimer.Interval = 1000;
            autoUpdateTimer.Tick += (s, e) => ReadValues();

            FlowLayoutPanel filePanel = new FlowLayoutPanel();
            filePanel.Dock = DockStyle.Top; filePanel.Height = 45; filePanel.BackColor = Color.LightGray; filePanel.Padding = new Padding(5);

            Button btnSave = new Button() { Text = "Сохранить", AutoSize = true };
            Button btnLoad = new Button() { Text = "Загрузить", AutoSize = true };
            btnSave.Click += (s, e) => SaveProject(null);
            btnLoad.Click += (s, e) => LoadProject(null);

            filePanel.Controls.AddRange(new Control[] { btnConnect, new Label { Width = 20 }, btnSave, btnLoad });

            TableLayoutPanel rootLayout = new TableLayoutPanel();
            rootLayout.Dock = DockStyle.Fill; rootLayout.RowCount = 3; rootLayout.ColumnCount = 1;
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            GroupBox gbServers = new GroupBox { Text = "1. ВЫБОР СЕРВЕРА", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 8, FontStyle.Bold) };
            gbServers.Controls.Add(lbServers);

            TableLayoutPanel bottomLayout = new TableLayoutPanel();
            bottomLayout.Dock = DockStyle.Fill; bottomLayout.ColumnCount = 2; bottomLayout.RowCount = 1;
            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));

            gbTags = new GroupBox { Text = "2. ТЕГИ", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 8, FontStyle.Bold) };
            gbTags.Controls.Add(lbTags); gbTags.Controls.Add(txtSearch);

            Panel favPanel = new Panel();
            favPanel.Dock = DockStyle.Fill;
            favPanel.Controls.Add(lvFavorites);
            favPanel.Controls.Add(btnMonitor);

            bottomLayout.Controls.Add(gbTags, 0, 0);
            bottomLayout.Controls.Add(favPanel, 1, 0);

            rootLayout.Controls.Add(filePanel, 0, 0);
            rootLayout.Controls.Add(gbServers, 0, 1);
            rootLayout.Controls.Add(bottomLayout, 0, 2);
            this.Controls.Add(rootLayout);

            this.Load += (s, e) => {
    // Создаем временный таймер для отложенного запуска
    Timer startupTimer = new Timer();
    startupTimer.Interval = 1000; // 1 секунда
    startupTimer.Tick += (sender, args) => {
        startupTimer.Stop();
        ScanServers();
        AutoLoadDefault();
        startupTimer.Dispose();
    };
    startupTimer.Start();
};

            btnConnect.Click += (s, e) => ConnectAndBrowse();
            lbTags.MouseDoubleClick += (s, e) => AddToFavorites();
            txtSearch.TextChanged += (s, e) => FilterTags();
            lvFavorites.MouseDoubleClick += (s, e) => EditFavoriteParams();
            btnMonitor.Click += (s, e) => { if (autoUpdateTimer.Enabled) StopMonitoring(); else StartMonitoring(); };

            ContextMenuStrip menuFav = new ContextMenuStrip();
            menuFav.Items.Add("Удалить тег", null, (s, e) => RemoveFromFavorites());
            lvFavorites.ContextMenuStrip = menuFav;
        }

        private void AddToFavorites()
        {
            if (lbTags.SelectedItem == null) return;
            string tag = lbTags.SelectedItem.ToString();
            using (TagEditorDialog diag = new TagEditorDialog(tag))
            {
                if (diag.ShowDialog() == DialogResult.OK)
                {
                    ListViewItem lvi = new ListViewItem(lvFavorites.Items.Count.ToString());
                    lvi.SubItems.AddRange(new[] { tag, "---", diag.K, diag.B, diag.SelectedType });
                    lvFavorites.Items.Add(lvi);
                    if (monitoringGroup != null)
                        monitoringGroup.AddItems(new[] { new Item { ItemName = tag } });
                }
            }
        }

        private void EditFavoriteParams()
        {
            if (lvFavorites.SelectedItems.Count == 0) return;
            ListViewItem row = lvFavorites.SelectedItems[0];
            using (TagEditorDialog diag = new TagEditorDialog(row.SubItems[1].Text, row.SubItems[3].Text, row.SubItems[4].Text, row.SubItems[5].Text))
            {
                if (diag.ShowDialog() == DialogResult.OK)
                {
                    row.SubItems[3].Text = diag.K;
                    row.SubItems[4].Text = diag.B;
                    row.SubItems[5].Text = diag.SelectedType;
                }
            }
        }

        private void RemoveFromFavorites()
        {
            if (lvFavorites.SelectedItems.Count > 0)
            {
                lvFavorites.Items.Remove(lvFavorites.SelectedItems[0]);
            }
        }

        private void SaveProject(string path)
        {
            string fn = path;
            if (fn == null)
            {
                using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "Text Files|*.txt", FileName = "tags.txt" })
                {
                    if (sfd.ShowDialog() == DialogResult.OK) fn = sfd.FileName;
                    else return;
                }
            }
            
            List<string> lines = new List<string>();
            foreach (ListViewItem it in lvFavorites.Items)
            {
                lines.Add(string.Format("{0}\t{1}\t{2}\t{3}\t{4}", it.Text, it.SubItems[1].Text, it.SubItems[3].Text, it.SubItems[4].Text, it.SubItems[5].Text));
            }
            File.WriteAllLines(fn, lines.ToArray());
            MessageBox.Show("Проект сохранен!", "Инфо", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadProject(string path)
        {
            string fn = path;
            if (fn == null)
            {
                using (OpenFileDialog ofd = new OpenFileDialog() { Filter = "Text Files|*.txt" })
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
                    ListViewItem lvi = new ListViewItem(p[0]);
                    lvi.SubItems.AddRange(new[] { p[1], "---", p[2], p[3], p[4] });
                    lvFavorites.Items.Add(lvi);
                    if (monitoringGroup != null)
                        monitoringGroup.AddItems(new[] { new Item { ItemName = p[1] } });
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
                    string rawVal = results[i].Value != null ? results[i].Value.ToString() : "";
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
                double v, k, b;
                if (double.TryParse(raw.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out v) &&
                    double.TryParse(ks.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out k) &&
                    double.TryParse(bs.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out b))
                {
                    return (v * k + b).ToString("F2");
                }
            }
            return raw;
        }

        private void AutoLoadDefault() { if (File.Exists(defaultFile)) LoadProject(defaultFile); }

        private void ConnectAndBrowse()
        {
            if (!(lbServers.SelectedItem is Opc.Server)) return;
            Opc.Server info = (Opc.Server)lbServers.SelectedItem;
            try {
                if (selectedServer != null) selectedServer.Disconnect();
                selectedServer = new Opc.Da.Server(new OpcCom.Factory(), null);
                selectedServer.Connect(info.Url, new ConnectData(new System.Net.NetworkCredential()));
                monitoringGroup = (Opc.Da.Subscription)selectedServer.CreateSubscription(new SubscriptionState { Name = "HmiGroup", Active = true });
                lbTags.Items.Clear(); fullTagList.Clear();
                BrowseRecursive(null); 
                UpdateTagCounter();
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

        private void BrowseRecursive(ItemIdentifier p)
        {
            // Исправлено: используем BrowsePosition вместо ItemValueResult[]
            Opc.Da.BrowsePosition position; 
            BrowseFilters filters = new BrowseFilters();
            filters.BrowseFilter = browseFilter.all;
            
            Opc.Da.BrowseElement[] e = selectedServer.Browse(p, filters, out position);
            
            if (e == null) return;
            foreach (var el in e) {
                if (el.IsItem) { 
                    lbTags.Items.Add(el.ItemName); 
                    fullTagList.Add(el.ItemName); 
                }
                if (el.HasChildren) {
                    BrowseRecursive(new ItemIdentifier(el.ItemPath, el.ItemName));
                }
            }
        }

        private void FilterTags()
        {
            lbTags.Items.Clear();
            foreach (string t in fullTagList) {
                if (t.IndexOf(txtSearch.Text, StringComparison.OrdinalIgnoreCase) >= 0) lbTags.Items.Add(t);
            }
            UpdateTagCounter();
        }

        private void UpdateTagCounter() { gbTags.Text = string.Format("2. ТЕГИ ({0}/{1})", lbTags.Items.Count, fullTagList.Count); }
        private void StartMonitoring() { autoUpdateTimer.Start(); btnMonitor.Text = "СТОП"; btnMonitor.BackColor = Color.MistyRose; }
        private void StopMonitoring() { autoUpdateTimer.Stop(); btnMonitor.Text = "СТАРТ МОНИТОРИНГ"; btnMonitor.BackColor = Color.LightGreen; }
    }
}
