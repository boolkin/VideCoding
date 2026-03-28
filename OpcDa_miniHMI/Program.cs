using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.IO;
using System.Text.Json;
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

    public class TagConfig
    {
        public string TagName { get; set; }
        public string Alias { get; set; }
        public string Unit { get; set; }
        public string K { get; set; }
        public string B { get; set; }
        public string Type { get; set; }
        public int X { get; set; } = -1;
        public int Y { get; set; } = -1;
    }

    public class MainForm : Form
    {
        private ListBox lbServers = new() { Dock = DockStyle.Fill, HorizontalScrollbar = true };
        private ListBox lbTags = new() { Dock = DockStyle.Fill };
        private ListView lvFavorites = new() { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true };
        
        // Холст с прокруткой
        private Panel pnlCanvasContainer = new() { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.DarkGray };
        private Panel pnlCanvas = new() { BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Location = new Point(10, 10), Size = new Size(1024, 768) };
        
        private Button btnMonitor = new() { Text = "СТАРТ МОНИТОРИНГ", Height = 40, Dock = DockStyle.Bottom, BackColor = Color.LightGreen, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
        private TextBox txtSearch = new() { Dock = DockStyle.Top, PlaceholderText = "Поиск тега..." };
        private GroupBox gbTags;
        private List<string> fullTagList = new();
        private System.Windows.Forms.Timer autoUpdateTimer = new();
        private Opc.Da.Server? selectedServer;
        private Opc.Da.Subscription? monitoringGroup;
        private Control? draggedControl;
        private Point mouseOffset;

        public MainForm()
        {
            this.Text = "OPC DA HMI Designer Pro";
            this.Size = new Size(1600, 950);

            lvFavorites.Columns.Add("#", 40);
            lvFavorites.Columns.Add("Тег (System Name)", 180);
            lvFavorites.Columns.Add("Название (Alias)", 150);
            lvFavorites.Columns.Add("Значение", 90);
            lvFavorites.Columns.Add("Ед. изм.", 60);
            lvFavorites.Columns.Add("K", 40);
            lvFavorites.Columns.Add("B", 40);
            lvFavorites.Columns.Add("Тип", 40);

            autoUpdateTimer.Interval = 1000;
            autoUpdateTimer.Tick += (s, e) => ReadValues();

            // Панель инструментов
            FlowLayoutPanel filePanel = new() { Dock = DockStyle.Top, Height = 40, BackColor = Color.LightGray };
            Button btnSave = new() { Text = "💾 Сохранить", AutoSize = true };
            Button btnLoad = new() { Text = "📂 Загрузить", AutoSize = true };
            Button btnCanvasSize = new() { Text = "📐 Размер холста", AutoSize = true };
            Button btnHtml = new() { Text = "🌐 HTML Экспорт", AutoSize = true, BackColor = Color.LightBlue };

            btnSave.Click += (s, e) => SaveProject();
            btnLoad.Click += (s, e) => LoadProject();
            btnHtml.Click += (s, e) => ExportToHtml();
            btnCanvasSize.Click += (s, e) => SetCanvasSize();

            filePanel.Controls.AddRange(new Control[] { btnSave, btnLoad, btnCanvasSize, btnHtml });

            TableLayoutPanel rootLayout = new() { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            rootLayout.Controls.Add(filePanel, 0, 0);
            GroupBox gbServers = new GroupBox { Text = "1. OPC СЕРВЕР", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 8, FontStyle.Bold) };
            gbServers.Controls.Add(lbServers);
            rootLayout.Controls.Add(gbServers, 0, 1);

            TableLayoutPanel bottomLayout = new() { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 2 };
            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            bottomLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));
            bottomLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            gbTags = new GroupBox { Text = "2. ТЕГИ", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 8, FontStyle.Bold) };
            gbTags.Controls.Add(lbTags);
            gbTags.Controls.Add(txtSearch);

            bottomLayout.Controls.Add(CreateLabel("ТЕГИ"), 0, 0);
            bottomLayout.Controls.Add(CreateLabel("ИЗБРАННОЕ"), 1, 0);
            bottomLayout.Controls.Add(CreateLabel("HMI ХОЛСТ (1024x768)"), 2, 0);
            bottomLayout.Controls.Add(gbTags, 0, 1);

            Panel favPanel = new() { Dock = DockStyle.Fill };
            favPanel.Controls.Add(lvFavorites);
            favPanel.Controls.Add(btnMonitor);
            bottomLayout.Controls.Add(favPanel, 1, 1);
            
            pnlCanvasContainer.Controls.Add(pnlCanvas);
            bottomLayout.Controls.Add(pnlCanvasContainer, 2, 1);

            rootLayout.Controls.Add(bottomLayout, 0, 2);
            this.Controls.Add(rootLayout);

            this.Load += (s, e) => ScanServers();
            lbServers.SelectedIndexChanged += (s, e) => ConnectAndBrowse();
            lbTags.MouseDoubleClick += (s, e) => AddToFavorites();
            txtSearch.TextChanged += (s, e) => FilterTags();
            lvFavorites.MouseDoubleClick += (s, e) => CreateHmiWidgetFromSelection();

            ContextMenuStrip menuFav = new();
            menuFav.Items.Add("Добавить на холст", null, (s, e) => CreateHmiWidgetFromSelection());
            menuFav.Items.Add("Редактировать параметры", null, (s, e) => EditFavoriteTag());
            menuFav.Items.Add(new ToolStripSeparator());
            menuFav.Items.Add("Удалить из списка", null, (s, e) => RemoveFromFavorites());
            lvFavorites.ContextMenuStrip = menuFav;

            btnMonitor.Click += (s, e) => { if (autoUpdateTimer.Enabled) StopMonitoring(); else StartMonitoring(); };
        }

        // --- НАСТРОЙКА РАЗМЕРА ХОЛСТА ---
        private void SetCanvasSize()
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox("Введите размер (Ширина x Высота):", "Настройка холста", $"{pnlCanvas.Width}x{pnlCanvas.Height}");
            if (string.IsNullOrEmpty(input)) return;
            var parts = input.Split('x');
            if (parts.Length == 2 && int.TryParse(parts[0], out int w) && int.TryParse(parts[1], out int h))
            {
                pnlCanvas.Size = new Size(w, h);
            }
        }

        // --- ОБНОВЛЕННЫЙ ВИДЖЕТ (100x40, ЧЕРНЫЙ ТЕКСТ) ---
        private void CreateHmiWidget(TagConfig cfg)
        {
            if (pnlCanvas.Controls.ContainsKey(cfg.TagName)) return;

            Panel widget = new() { 
                Name = cfg.TagName, 
                Size = new Size(100, 40), 
                BackColor = Color.White, 
                BorderStyle = BorderStyle.FixedSingle, 
                Location = new Point(cfg.X != -1 ? cfg.X : 20, cfg.Y != -1 ? cfg.Y : 20) 
            };

            ContextMenuStrip wMenu = new();
            wMenu.Items.Add("Удалить виджет", null, (s, e) => pnlCanvas.Controls.Remove(widget));
            widget.ContextMenuStrip = wMenu;

            // Название тега (Alias) сверху
            Label lblAlias = new() { 
                Text = cfg.Alias, 
                Dock = DockStyle.Top, 
                Height = 15, 
                Font = new Font("Segoe UI", 7, FontStyle.Bold), 
                TextAlign = ContentAlignment.TopCenter,
                ForeColor = Color.Black,
                Enabled = false 
            };

            // Значение + Единицы в центре (черный текст)
            Label lblVal = new() { 
                Name = "lblVal", 
                Tag = cfg.Unit, // Сохраняем единицы в Tag для обновления
                Text = $"0.0 {cfg.Unit}", 
                Dock = DockStyle.Fill, 
                Font = new Font("Segoe UI", 9, FontStyle.Bold), 
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Black,
                Enabled = false 
            };

            widget.Controls.Add(lblVal);
            widget.Controls.Add(lblAlias);

            // Drag & Drop
            widget.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) { draggedControl = widget; mouseOffset = e.Location; widget.BringToFront(); } };
            widget.MouseMove += (s, e) => { if (draggedControl == widget) { widget.Left += e.X - mouseOffset.X; widget.Top += e.Y - mouseOffset.Y; } };
            widget.MouseUp += (s, e) => { draggedControl = null; };

            pnlCanvas.Controls.Add(widget);
        }

        private void CreateHmiWidgetFromSelection()
        {
            if (lvFavorites.SelectedItems.Count == 0) return;
            var item = lvFavorites.SelectedItems[0];
            CreateHmiWidget(new TagConfig {
                TagName = item.SubItems[1].Text,
                Alias = item.SubItems[2].Text,
                Unit = item.SubItems[4].Text
            });
        }

        // --- ЛОГИКА ЧТЕНИЯ (ОБНОВЛЕНА ДЛЯ НОВЫХ ВИДЖЕТОВ) ---
        private void ReadValues()
        {
            if (selectedServer == null || monitoringGroup == null || monitoringGroup.Items.Length == 0) return;
            try {
                ItemValueResult[] results = selectedServer.Read(monitoringGroup.Items);
                for (int i = 0; i < results.Length; i++) {
                    string rawVal = results[i].Value?.ToString() ?? "0";
                    string name = monitoringGroup.Items[i].ItemName;
                    
                    foreach (ListViewItem it in lvFavorites.Items) {
                        if (it.SubItems[1].Text == name) {
                            string type = it.SubItems[7].Text.ToLower();
                            string finalVal = ProcessValue(rawVal, type, it.SubItems[5].Text, it.SubItems[6].Text);
                            
                            it.SubItems[3].Text = finalVal;
                            
                            // Обновление виджета на холсте
                            var widget = pnlCanvas.Controls.Find(name, false).FirstOrDefault();
                            var lbl = widget?.Controls.Find("lblVal", false).FirstOrDefault() as Label;
                            if (lbl != null) {
                                lbl.Text = $"{finalVal} {lbl.Tag}"; // Значение + единицы
                            }
                            break;
                        }
                    }
                }
            } catch { }
        }

        private string ProcessValue(string raw, string type, string ks, string bs) {
            if (type == "s") {
                double.TryParse(raw.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double v);
                double.TryParse(ks.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double k);
                double.TryParse(bs.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double b);
                return (v * k + b).ToString("F2");
            }
            if (type.Contains("b")) return (raw == "1" || raw.ToLower() == "true") ? "ВКЛ" : "ВЫКЛ";
            return raw;
        }

        // --- ОСТАЛЬНЫЕ МЕТОДЫ (Scan, Connect, Save, Load) ---
        private void SaveProject() {
            SaveFileDialog sfd = new() { Filter = "HMI Project|*.json" };
            if (sfd.ShowDialog() != DialogResult.OK) return;
            var configs = lvFavorites.Items.Cast<ListViewItem>().Select(it => new TagConfig {
                TagName = it.SubItems[1].Text, Alias = it.SubItems[2].Text, Unit = it.SubItems[4].Text,
                K = it.SubItems[5].Text, B = it.SubItems[6].Text, Type = it.SubItems[7].Text,
                X = pnlCanvas.Controls.Find(it.SubItems[1].Text, false).FirstOrDefault()?.Left ?? -1,
                Y = pnlCanvas.Controls.Find(it.SubItems[1].Text, false).FirstOrDefault()?.Top ?? -1
            }).ToList();
            File.WriteAllText(sfd.FileName, JsonSerializer.Serialize(configs, new JsonSerializerOptions { WriteIndented = true }));
            MessageBox.Show("Проект сохранен!");
        }

        private void LoadProject() {
            OpenFileDialog ofd = new() { Filter = "HMI Project|*.json" };
            if (ofd.ShowDialog() != DialogResult.OK || monitoringGroup == null) return;
            var configs = JsonSerializer.Deserialize<List<TagConfig>>(File.ReadAllText(ofd.FileName));
            lvFavorites.Items.Clear(); pnlCanvas.Controls.Clear();
            foreach (var cfg in configs) {
                if (monitoringGroup.AddItems(new[] { new Item { ItemName = cfg.TagName } })[0].ResultID.Succeeded()) {
                    ListViewItem lvi = new((lvFavorites.Items.Count + 1).ToString());
                    lvi.SubItems.AddRange(new[] { cfg.TagName, cfg.Alias, "---", cfg.Unit, cfg.K, cfg.B, cfg.Type });
                    lvFavorites.Items.Add(lvi);
                    if (cfg.X != -1) CreateHmiWidget(cfg);
                }
            }
        }

        private void ScanServers() {
            try {
                var servers = new OpcCom.ServerEnumerator().GetAvailableServers(Specification.COM_DA_20);
                lbServers.Items.Clear();
                if (servers != null) { lbServers.Items.AddRange(servers); lbServers.DisplayMember = "Name"; }
            } catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void ConnectAndBrowse() {
            if (lbServers.SelectedItem is not Opc.Server info) return;
            try {
                if (selectedServer != null) selectedServer.Disconnect();
                selectedServer = new Opc.Da.Server(new OpcCom.Factory(), null);
                selectedServer.Connect(info.Url, new ConnectData(new System.Net.NetworkCredential()));
                monitoringGroup = (Opc.Da.Subscription)selectedServer.CreateSubscription(new SubscriptionState { Name = "HmiGroup", Active = true });
                lbTags.Items.Clear(); fullTagList.Clear(); BrowseRecursive(null); UpdateTagCounter();
            } catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void BrowseRecursive(ItemIdentifier? parent) {
            var elements = selectedServer?.Browse(parent, new BrowseFilters { BrowseFilter = browseFilter.all }, out _);
            if (elements == null) return;
            foreach (var el in elements) {
                if (el.IsItem) { lbTags.Items.Add(el.ItemName); fullTagList.Add(el.ItemName); }
                if (el.HasChildren) BrowseRecursive(new ItemIdentifier(el.ItemPath, el.ItemName));
            }
        }

        private void FilterTags() {
            lbTags.Items.Clear();
            lbTags.Items.AddRange(fullTagList.Where(t => t.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase)).ToArray());
            UpdateTagCounter();
        }

        private void UpdateTagCounter() => gbTags.Text = $"ТЕГИ ({lbTags.Items.Count}/{fullTagList.Count})";

        private void AddToFavorites() {
            if (lbTags.SelectedItem == null || monitoringGroup == null) return;
            string tag = lbTags.SelectedItem.ToString();
            string alias = Microsoft.VisualBasic.Interaction.InputBox("Имя:", "Настройка", tag);
            if (monitoringGroup.AddItems(new[] { new Item { ItemName = tag } })[0].ResultID.Succeeded()) {
                ListViewItem lvi = new((lvFavorites.Items.Count + 1).ToString());
                lvi.SubItems.AddRange(new[] { tag, alias, "---", "", "1", "0", "s" });
                lvFavorites.Items.Add(lvi);
            }
        }

        private void EditFavoriteTag() {
            if (lvFavorites.SelectedItems.Count == 0) return;
            var row = lvFavorites.SelectedItems[0];
            row.SubItems[2].Text = Microsoft.VisualBasic.Interaction.InputBox("Alias:", "Ред.", row.SubItems[2].Text);
            row.SubItems[4].Text = Microsoft.VisualBasic.Interaction.InputBox("Unit:", "Ред.", row.SubItems[4].Text);
        }

        private void RemoveFromFavorites() {
            if (lvFavorites.SelectedItems.Count == 0) return;
            var item = lvFavorites.SelectedItems[0];
            lvFavorites.Items.Remove(item);
            pnlCanvas.Controls.RemoveByKey(item.SubItems[1].Text);
        }

        private void StartMonitoring() { autoUpdateTimer.Start(); btnMonitor.Text = "СТОП"; btnMonitor.BackColor = Color.MistyRose; }
        private void StopMonitoring() { autoUpdateTimer.Stop(); btnMonitor.Text = "СТАРТ МОНИТОРИНГ"; btnMonitor.BackColor = Color.LightGreen; }
        private Label CreateLabel(string txt) => new Label { Text = txt, TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 8, FontStyle.Bold) };
        
        private void ExportToHtml() { MessageBox.Show("Экспорт временно отключен для обновления стилей."); }
    }
}
