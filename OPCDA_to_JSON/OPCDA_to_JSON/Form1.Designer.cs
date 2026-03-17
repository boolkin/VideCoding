namespace OPCWinFormsClient
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageTags = new System.Windows.Forms.TabPage();
            this.splitContainerMain = new System.Windows.Forms.SplitContainer();
            this.panelMiddle = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.listBoxAvailableTags = new System.Windows.Forms.ListBox();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.btnAddAll = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnRemoveAll = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.panelRight = new System.Windows.Forms.Panel();
            this.groupBoxCoefficients = new System.Windows.Forms.GroupBox();
            this.cmbType = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtUpdateRate = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtValue = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnMoveDown = new System.Windows.Forms.Button();
            this.btnMoveUp = new System.Windows.Forms.Button();
            this.btnExportTags = new System.Windows.Forms.Button();
            this.btnImportTags = new System.Windows.Forms.Button();
            this.listBoxSelectedTags = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.panelTop = new System.Windows.Forms.Panel();
            this.btnRefreshServers = new System.Windows.Forms.Button();
            this.listBoxServers = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPageConfig = new System.Windows.Forms.TabPage();
            this.groupBoxUdp = new System.Windows.Forms.GroupBox();
            this.txtTags2Send = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.txtRemoteIP = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.txtRemotePort = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.cmbUdpSend = new System.Windows.Forms.ComboBox();
            this.label11 = new System.Windows.Forms.Label();
            this.groupBoxWebServer = new System.Windows.Forms.GroupBox();
            this.txtPortNumber = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.cmbWebServer = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.groupBoxConsole = new System.Windows.Forms.GroupBox();
            this.cmbConsoleOutput = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBoxOpc = new System.Windows.Forms.GroupBox();
            this.txtRefreshTime = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.cmbOpcId = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.btnExportConfig = new System.Windows.Forms.Button();
            this.btnLoadConfig = new System.Windows.Forms.Button();
            this.statusStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPageTags.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
            this.splitContainerMain.Panel1.SuspendLayout();
            this.splitContainerMain.Panel2.SuspendLayout();
            this.splitContainerMain.SuspendLayout();
            this.panelMiddle.SuspendLayout();
            this.panelButtons.SuspendLayout();
            this.panelRight.SuspendLayout();
            this.groupBoxCoefficients.SuspendLayout();
            this.panelTop.SuspendLayout();
            this.tabPageConfig.SuspendLayout();
            this.groupBoxUdp.SuspendLayout();
            this.groupBoxWebServer.SuspendLayout();
            this.groupBoxConsole.SuspendLayout();
            this.groupBoxOpc.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 539);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 10, 0);
            this.statusStrip1.Size = new System.Drawing.Size(982, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(88, 17);
            this.toolStripStatusLabel.Text = "Готов к работе";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageTags);
            this.tabControl1.Controls.Add(this.tabPageConfig);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(982, 539);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPageTags
            // 
            this.tabPageTags.Controls.Add(this.splitContainerMain);
            this.tabPageTags.Controls.Add(this.panelTop);
            this.tabPageTags.Location = new System.Drawing.Point(4, 22);
            this.tabPageTags.Margin = new System.Windows.Forms.Padding(2);
            this.tabPageTags.Name = "tabPageTags";
            this.tabPageTags.Padding = new System.Windows.Forms.Padding(2);
            this.tabPageTags.Size = new System.Drawing.Size(974, 513);
            this.tabPageTags.TabIndex = 0;
            this.tabPageTags.Text = "Добавление тегов";
            this.tabPageTags.UseVisualStyleBackColor = true;
            // 
            // splitContainerMain
            // 
            this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMain.Location = new System.Drawing.Point(2, 124);
            this.splitContainerMain.Name = "splitContainerMain";
            // 
            // splitContainerMain.Panel1
            // 
            this.splitContainerMain.Panel1.Controls.Add(this.panelMiddle);
            // 
            // splitContainerMain.Panel2
            // 
            this.splitContainerMain.Panel2.Controls.Add(this.panelButtons);
            this.splitContainerMain.Panel2.Controls.Add(this.panelRight);
            this.splitContainerMain.Size = new System.Drawing.Size(970, 387);
            this.splitContainerMain.SplitterDistance = 480;
            this.splitContainerMain.TabIndex = 6;
            // 
            // panelMiddle
            // 
            this.panelMiddle.Controls.Add(this.label2);
            this.panelMiddle.Controls.Add(this.txtSearch);
            this.panelMiddle.Controls.Add(this.listBoxAvailableTags);
            this.panelMiddle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMiddle.Location = new System.Drawing.Point(0, 0);
            this.panelMiddle.Margin = new System.Windows.Forms.Padding(2);
            this.panelMiddle.Name = "panelMiddle";
            this.panelMiddle.Size = new System.Drawing.Size(480, 387);
            this.panelMiddle.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 36);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(92, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Доступные теги:";
            // 
            // txtSearch
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Location = new System.Drawing.Point(12, 7);
            this.txtSearch.Margin = new System.Windows.Forms.Padding(2);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(460, 20);
            this.txtSearch.TabIndex = 1;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // listBoxAvailableTags
            // 
            this.listBoxAvailableTags.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxAvailableTags.FormattingEnabled = true;
            this.listBoxAvailableTags.HorizontalScrollbar = true;
            this.listBoxAvailableTags.Location = new System.Drawing.Point(9, 54);
            this.listBoxAvailableTags.Margin = new System.Windows.Forms.Padding(2);
            this.listBoxAvailableTags.Name = "listBoxAvailableTags";
            this.listBoxAvailableTags.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxAvailableTags.Size = new System.Drawing.Size(463, 329);
            this.listBoxAvailableTags.TabIndex = 0;
            this.listBoxAvailableTags.DoubleClick += new System.EventHandler(this.listBoxAvailableTags_DoubleClick);
            // 
            // panelButtons
            // 
            this.panelButtons.Controls.Add(this.btnAddAll);
            this.panelButtons.Controls.Add(this.btnAdd);
            this.panelButtons.Controls.Add(this.btnRemoveAll);
            this.panelButtons.Controls.Add(this.btnRemove);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelButtons.Location = new System.Drawing.Point(0, 0);
            this.panelButtons.Margin = new System.Windows.Forms.Padding(2);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(55, 387);
            this.panelButtons.TabIndex = 4;
            // 
            // btnAddAll
            // 
            this.btnAddAll.Location = new System.Drawing.Point(4, 203);
            this.btnAddAll.Margin = new System.Windows.Forms.Padding(2);
            this.btnAddAll.Name = "btnAddAll";
            this.btnAddAll.Size = new System.Drawing.Size(47, 24);
            this.btnAddAll.TabIndex = 3;
            this.btnAddAll.Text = ">> Все";
            this.btnAddAll.UseVisualStyleBackColor = true;
            this.btnAddAll.Click += new System.EventHandler(this.btnAddAll_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(4, 171);
            this.btnAdd.Margin = new System.Windows.Forms.Padding(2);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(47, 24);
            this.btnAdd.TabIndex = 2;
            this.btnAdd.Text = ">";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnRemoveAll
            // 
            this.btnRemoveAll.Location = new System.Drawing.Point(4, 122);
            this.btnRemoveAll.Margin = new System.Windows.Forms.Padding(2);
            this.btnRemoveAll.Name = "btnRemoveAll";
            this.btnRemoveAll.Size = new System.Drawing.Size(47, 24);
            this.btnRemoveAll.TabIndex = 1;
            this.btnRemoveAll.Text = "<< Все";
            this.btnRemoveAll.UseVisualStyleBackColor = true;
            this.btnRemoveAll.Click += new System.EventHandler(this.btnRemoveAll_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.Location = new System.Drawing.Point(4, 89);
            this.btnRemove.Margin = new System.Windows.Forms.Padding(2);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(47, 24);
            this.btnRemove.TabIndex = 0;
            this.btnRemove.Text = "<";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // panelRight
            // 
            this.panelRight.Controls.Add(this.groupBoxCoefficients);
            this.panelRight.Controls.Add(this.btnMoveDown);
            this.panelRight.Controls.Add(this.btnMoveUp);
            this.panelRight.Controls.Add(this.btnExportTags);
            this.panelRight.Controls.Add(this.btnImportTags);
            this.panelRight.Controls.Add(this.listBoxSelectedTags);
            this.panelRight.Controls.Add(this.label3);
            this.panelRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRight.Location = new System.Drawing.Point(0, 0);
            this.panelRight.Margin = new System.Windows.Forms.Padding(2);
            this.panelRight.Name = "panelRight";
            this.panelRight.Size = new System.Drawing.Size(486, 387);
            this.panelRight.TabIndex = 3;
            // 
            // groupBoxCoefficients
            // 
            this.groupBoxCoefficients.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxCoefficients.Controls.Add(this.cmbType);
            this.groupBoxCoefficients.Controls.Add(this.label6);
            this.groupBoxCoefficients.Controls.Add(this.txtUpdateRate);
            this.groupBoxCoefficients.Controls.Add(this.label5);
            this.groupBoxCoefficients.Controls.Add(this.txtValue);
            this.groupBoxCoefficients.Controls.Add(this.label4);
            this.groupBoxCoefficients.Location = new System.Drawing.Point(59, 312);
            this.groupBoxCoefficients.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxCoefficients.Name = "groupBoxCoefficients";
            this.groupBoxCoefficients.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxCoefficients.Size = new System.Drawing.Size(420, 68);
            this.groupBoxCoefficients.TabIndex = 8;
            this.groupBoxCoefficients.TabStop = false;
            this.groupBoxCoefficients.Text = "Коэффициенты тега";
            // 
            // cmbType
            // 
            this.cmbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbType.FormattingEnabled = true;
            this.cmbType.Location = new System.Drawing.Point(326, 36);
            this.cmbType.Margin = new System.Windows.Forms.Padding(2);
            this.cmbType.Name = "cmbType";
            this.cmbType.Size = new System.Drawing.Size(90, 21);
            this.cmbType.TabIndex = 5;
            this.cmbType.SelectedIndexChanged += new System.EventHandler(this.cmbType_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(324, 19);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(29, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "Тип:";
            // 
            // txtUpdateRate
            // 
            this.txtUpdateRate.Location = new System.Drawing.Point(176, 37);
            this.txtUpdateRate.Margin = new System.Windows.Forms.Padding(2);
            this.txtUpdateRate.Name = "txtUpdateRate";
            this.txtUpdateRate.Size = new System.Drawing.Size(134, 20);
            this.txtUpdateRate.TabIndex = 3;
            this.txtUpdateRate.TextChanged += new System.EventHandler(this.txtUpdateRate_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(174, 20);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(64, 13);
            this.label5.TabIndex = 2;
            this.label5.Text = "Смещение:";
            // 
            // txtValue
            // 
            this.txtValue.Location = new System.Drawing.Point(12, 37);
            this.txtValue.Margin = new System.Windows.Forms.Padding(2);
            this.txtValue.Name = "txtValue";
            this.txtValue.Size = new System.Drawing.Size(148, 20);
            this.txtValue.TabIndex = 1;
            this.txtValue.TextChanged += new System.EventHandler(this.txtValue_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 20);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(80, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Коэффициент:";
            // 
            // btnMoveDown
            // 
            this.btnMoveDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMoveDown.Location = new System.Drawing.Point(409, 89);
            this.btnMoveDown.Margin = new System.Windows.Forms.Padding(2);
            this.btnMoveDown.Name = "btnMoveDown";
            this.btnMoveDown.Size = new System.Drawing.Size(70, 24);
            this.btnMoveDown.TabIndex = 7;
            this.btnMoveDown.Text = "Вниз";
            this.btnMoveDown.UseVisualStyleBackColor = true;
            this.btnMoveDown.Click += new System.EventHandler(this.btnMoveDown_Click);
            // 
            // btnMoveUp
            // 
            this.btnMoveUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMoveUp.Location = new System.Drawing.Point(409, 62);
            this.btnMoveUp.Margin = new System.Windows.Forms.Padding(2);
            this.btnMoveUp.Name = "btnMoveUp";
            this.btnMoveUp.Size = new System.Drawing.Size(70, 24);
            this.btnMoveUp.TabIndex = 6;
            this.btnMoveUp.Text = "Вверх";
            this.btnMoveUp.UseVisualStyleBackColor = true;
            this.btnMoveUp.Click += new System.EventHandler(this.btnMoveUp_Click);
            // 
            // btnExportTags
            // 
            this.btnExportTags.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExportTags.Location = new System.Drawing.Point(149, 7);
            this.btnExportTags.Margin = new System.Windows.Forms.Padding(2);
            this.btnExportTags.Name = "btnExportTags";
            this.btnExportTags.Size = new System.Drawing.Size(73, 24);
            this.btnExportTags.TabIndex = 4;
            this.btnExportTags.Text = "Экспорт";
            this.btnExportTags.UseVisualStyleBackColor = true;
            this.btnExportTags.Click += new System.EventHandler(this.btnExportTags_Click);
            // 
            // btnImportTags
            // 
            this.btnImportTags.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnImportTags.Location = new System.Drawing.Point(62, 7);
            this.btnImportTags.Margin = new System.Windows.Forms.Padding(2);
            this.btnImportTags.Name = "btnImportTags";
            this.btnImportTags.Size = new System.Drawing.Size(73, 24);
            this.btnImportTags.TabIndex = 3;
            this.btnImportTags.Text = "Импорт";
            this.btnImportTags.UseVisualStyleBackColor = true;
            this.btnImportTags.Click += new System.EventHandler(this.btnImportTags_Click);
            // 
            // listBoxSelectedTags
            // 
            this.listBoxSelectedTags.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxSelectedTags.FormattingEnabled = true;
            this.listBoxSelectedTags.HorizontalScrollbar = true;
            this.listBoxSelectedTags.Location = new System.Drawing.Point(59, 54);
            this.listBoxSelectedTags.Margin = new System.Windows.Forms.Padding(2);
            this.listBoxSelectedTags.Name = "listBoxSelectedTags";
            this.listBoxSelectedTags.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxSelectedTags.Size = new System.Drawing.Size(346, 251);
            this.listBoxSelectedTags.TabIndex = 1;
            this.listBoxSelectedTags.SelectedIndexChanged += new System.EventHandler(this.listBoxSelectedTags_SelectedIndexChanged);
            this.listBoxSelectedTags.DoubleClick += new System.EventHandler(this.listBoxSelectedTags_DoubleClick);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(59, 36);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(94, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Выбранные теги:";
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.btnRefreshServers);
            this.panelTop.Controls.Add(this.listBoxServers);
            this.panelTop.Controls.Add(this.label1);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(2, 2);
            this.panelTop.Margin = new System.Windows.Forms.Padding(2);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(970, 122);
            this.panelTop.TabIndex = 5;
            // 
            // btnRefreshServers
            // 
            this.btnRefreshServers.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefreshServers.Location = new System.Drawing.Point(892, 10);
            this.btnRefreshServers.Margin = new System.Windows.Forms.Padding(2);
            this.btnRefreshServers.Name = "btnRefreshServers";
            this.btnRefreshServers.Size = new System.Drawing.Size(73, 19);
            this.btnRefreshServers.TabIndex = 2;
            this.btnRefreshServers.Text = "Обновить";
            this.btnRefreshServers.UseVisualStyleBackColor = true;
            this.btnRefreshServers.Click += new System.EventHandler(this.btnRefreshServers_Click);
            // 
            // listBoxServers
            // 
            this.listBoxServers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxServers.FormattingEnabled = true;
            this.listBoxServers.HorizontalScrollbar = true;
            this.listBoxServers.Location = new System.Drawing.Point(9, 31);
            this.listBoxServers.Margin = new System.Windows.Forms.Padding(2);
            this.listBoxServers.Name = "listBoxServers";
            this.listBoxServers.Size = new System.Drawing.Size(956, 82);
            this.listBoxServers.TabIndex = 1;
            this.listBoxServers.DoubleClick += new System.EventHandler(this.listBoxServers_DoubleClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 10);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(281, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "OPC Сервера (дважды щелкните для загрузки тегов):";
            // 
            // tabPageConfig
            // 
            this.tabPageConfig.Controls.Add(this.groupBoxUdp);
            this.tabPageConfig.Controls.Add(this.groupBoxWebServer);
            this.tabPageConfig.Controls.Add(this.groupBoxConsole);
            this.tabPageConfig.Controls.Add(this.groupBoxOpc);
            this.tabPageConfig.Controls.Add(this.btnExportConfig);
            this.tabPageConfig.Controls.Add(this.btnLoadConfig);
            this.tabPageConfig.Location = new System.Drawing.Point(4, 22);
            this.tabPageConfig.Margin = new System.Windows.Forms.Padding(2);
            this.tabPageConfig.Name = "tabPageConfig";
            this.tabPageConfig.Padding = new System.Windows.Forms.Padding(2);
            this.tabPageConfig.Size = new System.Drawing.Size(974, 513);
            this.tabPageConfig.TabIndex = 1;
            this.tabPageConfig.Text = "Конфигурация";
            this.tabPageConfig.UseVisualStyleBackColor = true;
            // 
            // groupBoxUdp
            // 
            this.groupBoxUdp.Controls.Add(this.txtTags2Send);
            this.groupBoxUdp.Controls.Add(this.label14);
            this.groupBoxUdp.Controls.Add(this.txtRemoteIP);
            this.groupBoxUdp.Controls.Add(this.label13);
            this.groupBoxUdp.Controls.Add(this.txtRemotePort);
            this.groupBoxUdp.Controls.Add(this.label12);
            this.groupBoxUdp.Controls.Add(this.cmbUdpSend);
            this.groupBoxUdp.Controls.Add(this.label11);
            this.groupBoxUdp.Location = new System.Drawing.Point(20, 320);
            this.groupBoxUdp.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxUdp.Name = "groupBoxUdp";
            this.groupBoxUdp.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxUdp.Size = new System.Drawing.Size(400, 150);
            this.groupBoxUdp.TabIndex = 7;
            this.groupBoxUdp.TabStop = false;
            this.groupBoxUdp.Text = "Настройки UDP";
            // 
            // txtTags2Send
            // 
            this.txtTags2Send.Location = new System.Drawing.Point(120, 110);
            this.txtTags2Send.Margin = new System.Windows.Forms.Padding(2);
            this.txtTags2Send.Name = "txtTags2Send";
            this.txtTags2Send.Size = new System.Drawing.Size(100, 20);
            this.txtTags2Send.TabIndex = 7;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(15, 112);
            this.label14.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(98, 13);
            this.label14.TabIndex = 6;
            this.label14.Text = "Количество тегов:";
            // 
            // txtRemoteIP
            // 
            this.txtRemoteIP.Location = new System.Drawing.Point(120, 80);
            this.txtRemoteIP.Margin = new System.Windows.Forms.Padding(2);
            this.txtRemoteIP.Name = "txtRemoteIP";
            this.txtRemoteIP.Size = new System.Drawing.Size(100, 20);
            this.txtRemoteIP.TabIndex = 5;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(15, 82);
            this.label13.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(83, 13);
            this.label13.TabIndex = 4;
            this.label13.Text = "Удаленный IP:";
            // 
            // txtRemotePort
            // 
            this.txtRemotePort.Location = new System.Drawing.Point(120, 50);
            this.txtRemotePort.Margin = new System.Windows.Forms.Padding(2);
            this.txtRemotePort.Name = "txtRemotePort";
            this.txtRemotePort.Size = new System.Drawing.Size(100, 20);
            this.txtRemotePort.TabIndex = 3;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(15, 52);
            this.label12.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(84, 13);
            this.label12.TabIndex = 2;
            this.label12.Text = "Удаленный порт:";
            // 
            // cmbUdpSend
            // 
            this.cmbUdpSend.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbUdpSend.FormattingEnabled = true;
            this.cmbUdpSend.Location = new System.Drawing.Point(120, 20);
            this.cmbUdpSend.Margin = new System.Windows.Forms.Padding(2);
            this.cmbUdpSend.Name = "cmbUdpSend";
            this.cmbUdpSend.Size = new System.Drawing.Size(100, 21);
            this.cmbUdpSend.TabIndex = 1;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(15, 22);
            this.label11.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(32, 13);
            this.label11.TabIndex = 0;
            this.label11.Text = "UDP:";
            // 
            // groupBoxWebServer
            // 
            this.groupBoxWebServer.Controls.Add(this.txtPortNumber);
            this.groupBoxWebServer.Controls.Add(this.label10);
            this.groupBoxWebServer.Controls.Add(this.cmbWebServer);
            this.groupBoxWebServer.Controls.Add(this.label9);
            this.groupBoxWebServer.Location = new System.Drawing.Point(20, 200);
            this.groupBoxWebServer.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxWebServer.Name = "groupBoxWebServer";
            this.groupBoxWebServer.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxWebServer.Size = new System.Drawing.Size(400, 100);
            this.groupBoxWebServer.TabIndex = 6;
            this.groupBoxWebServer.TabStop = false;
            this.groupBoxWebServer.Text = "Настройки веб-сервера";
            // 
            // txtPortNumber
            // 
            this.txtPortNumber.Location = new System.Drawing.Point(120, 60);
            this.txtPortNumber.Margin = new System.Windows.Forms.Padding(2);
            this.txtPortNumber.Name = "txtPortNumber";
            this.txtPortNumber.Size = new System.Drawing.Size(100, 20);
            this.txtPortNumber.TabIndex = 3;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(15, 62);
            this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(76, 13);
            this.label10.TabIndex = 2;
            this.label10.Text = "Номер порта:";
            // 
            // cmbWebServer
            // 
            this.cmbWebServer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbWebServer.FormattingEnabled = true;
            this.cmbWebServer.Location = new System.Drawing.Point(120, 30);
            this.cmbWebServer.Margin = new System.Windows.Forms.Padding(2);
            this.cmbWebServer.Name = "cmbWebServer";
            this.cmbWebServer.Size = new System.Drawing.Size(100, 21);
            this.cmbWebServer.TabIndex = 1;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(15, 32);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(71, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "Веб-сервер:";
            // 
            // groupBoxConsole
            // 
            this.groupBoxConsole.Controls.Add(this.cmbConsoleOutput);
            this.groupBoxConsole.Controls.Add(this.label8);
            this.groupBoxConsole.Location = new System.Drawing.Point(20, 120);
            this.groupBoxConsole.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxConsole.Name = "groupBoxConsole";
            this.groupBoxConsole.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxConsole.Size = new System.Drawing.Size(400, 60);
            this.groupBoxConsole.TabIndex = 5;
            this.groupBoxConsole.TabStop = false;
            this.groupBoxConsole.Text = "Настройки консоли";
            // 
            // cmbConsoleOutput
            // 
            this.cmbConsoleOutput.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbConsoleOutput.FormattingEnabled = true;
            this.cmbConsoleOutput.Location = new System.Drawing.Point(120, 25);
            this.cmbConsoleOutput.Margin = new System.Windows.Forms.Padding(2);
            this.cmbConsoleOutput.Name = "cmbConsoleOutput";
            this.cmbConsoleOutput.Size = new System.Drawing.Size(100, 21);
            this.cmbConsoleOutput.TabIndex = 1;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(15, 27);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(94, 13);
            this.label8.TabIndex = 0;
            this.label8.Text = "Вывод в консоль:";
            // 
            // groupBoxOpc
            // 
            this.groupBoxOpc.Controls.Add(this.txtRefreshTime);
            this.groupBoxOpc.Controls.Add(this.label15);
            this.groupBoxOpc.Controls.Add(this.cmbOpcId);
            this.groupBoxOpc.Controls.Add(this.label7);
            this.groupBoxOpc.Location = new System.Drawing.Point(20, 20);
            this.groupBoxOpc.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxOpc.Name = "groupBoxOpc";
            this.groupBoxOpc.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxOpc.Size = new System.Drawing.Size(400, 90);
            this.groupBoxOpc.TabIndex = 4;
            this.groupBoxOpc.TabStop = false;
            this.groupBoxOpc.Text = "Настройки OPC";
            // 
            // txtRefreshTime
            // 
            this.txtRefreshTime.Location = new System.Drawing.Point(120, 55);
            this.txtRefreshTime.Margin = new System.Windows.Forms.Padding(2);
            this.txtRefreshTime.Name = "txtRefreshTime";
            this.txtRefreshTime.Size = new System.Drawing.Size(100, 20);
            this.txtRefreshTime.TabIndex = 3;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(15, 57);
            this.label15.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(94, 13);
            this.label15.TabIndex = 2;
            this.label15.Text = "Время обновления:";
            // 
            // cmbOpcId
            // 
            this.cmbOpcId.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbOpcId.FormattingEnabled = true;
            this.cmbOpcId.Location = new System.Drawing.Point(120, 25);
            this.cmbOpcId.Margin = new System.Windows.Forms.Padding(2);
            this.cmbOpcId.Name = "cmbOpcId";
            this.cmbOpcId.Size = new System.Drawing.Size(200, 21);
            this.cmbOpcId.TabIndex = 1;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(15, 27);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(72, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "OPC сервер:";
            // 
            // btnExportConfig
            // 
            this.btnExportConfig.Location = new System.Drawing.Point(200, 480);
            this.btnExportConfig.Margin = new System.Windows.Forms.Padding(2);
            this.btnExportConfig.Name = "btnExportConfig";
            this.btnExportConfig.Size = new System.Drawing.Size(150, 25);
            this.btnExportConfig.TabIndex = 1;
            this.btnExportConfig.Text = "Экспорт конфигурации";
            this.btnExportConfig.UseVisualStyleBackColor = true;
            this.btnExportConfig.Click += new System.EventHandler(this.btnExportConfig_Click);
            // 
            // btnLoadConfig
            // 
            this.btnLoadConfig.Location = new System.Drawing.Point(20, 480);
            this.btnLoadConfig.Margin = new System.Windows.Forms.Padding(2);
            this.btnLoadConfig.Name = "btnLoadConfig";
            this.btnLoadConfig.Size = new System.Drawing.Size(150, 25);
            this.btnLoadConfig.TabIndex = 0;
            this.btnLoadConfig.Text = "Загрузить конфигурацию";
            this.btnLoadConfig.UseVisualStyleBackColor = true;
            this.btnLoadConfig.Click += new System.EventHandler(this.btnLoadConfig_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(982, 561);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(750, 500);
            this.Name = "Form1";
            this.Text = "OPC Client - Теги";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPageTags.ResumeLayout(false);
            this.splitContainerMain.Panel1.ResumeLayout(false);
            this.splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
            this.splitContainerMain.ResumeLayout(false);
            this.panelMiddle.ResumeLayout(false);
            this.panelMiddle.PerformLayout();
            this.panelButtons.ResumeLayout(false);
            this.panelRight.ResumeLayout(false);
            this.panelRight.PerformLayout();
            this.groupBoxCoefficients.ResumeLayout(false);
            this.groupBoxCoefficients.PerformLayout();
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.tabPageConfig.ResumeLayout(false);
            this.groupBoxUdp.ResumeLayout(false);
            this.groupBoxUdp.PerformLayout();
            this.groupBoxWebServer.ResumeLayout(false);
            this.groupBoxWebServer.PerformLayout();
            this.groupBoxConsole.ResumeLayout(false);
            this.groupBoxConsole.PerformLayout();
            this.groupBoxOpc.ResumeLayout(false);
            this.groupBoxOpc.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageTags;
        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.Panel panelMiddle;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.ListBox listBoxAvailableTags;
        private System.Windows.Forms.Panel panelButtons;
        private System.Windows.Forms.Button btnAddAll;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnRemoveAll;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.GroupBox groupBoxCoefficients;
        private System.Windows.Forms.ComboBox cmbType;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtUpdateRate;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtValue;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnMoveDown;
        private System.Windows.Forms.Button btnMoveUp;
        private System.Windows.Forms.Button btnExportTags;
        private System.Windows.Forms.Button btnImportTags;
        private System.Windows.Forms.ListBox listBoxSelectedTags;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Button btnRefreshServers;
        private System.Windows.Forms.ListBox listBoxServers;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabPageConfig;
        private System.Windows.Forms.Button btnLoadConfig;
        private System.Windows.Forms.Button btnExportConfig;
        private System.Windows.Forms.GroupBox groupBoxOpc;
        private System.Windows.Forms.ComboBox cmbOpcId;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtRefreshTime;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.GroupBox groupBoxConsole;
        private System.Windows.Forms.ComboBox cmbConsoleOutput;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox groupBoxWebServer;
        private System.Windows.Forms.TextBox txtPortNumber;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox cmbWebServer;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.GroupBox groupBoxUdp;
        private System.Windows.Forms.TextBox txtTags2Send;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox txtRemoteIP;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtRemotePort;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ComboBox cmbUdpSend;
        private System.Windows.Forms.Label label11;
    }
}