namespace DocumentSearcher
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBoxFolderPath = new System.Windows.Forms.TextBox();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.panelDropTarget = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxSearch = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.checkBoxTxt = new System.Windows.Forms.CheckBox();
            this.checkBoxDocx = new System.Windows.Forms.CheckBox();
            this.checkBoxXlsx = new System.Windows.Forms.CheckBox();
            this.buttonSearch = new System.Windows.Forms.Button();
            this.listBoxResults = new System.Windows.Forms.ListBox();
            this.progressBarSearch = new System.Windows.Forms.ProgressBar();
            this.labelProgress = new System.Windows.Forms.Label();
            this.labelFileCount = new System.Windows.Forms.Label();
            this.labelFoundCount = new System.Windows.Forms.Label();
            this.backgroundWorkerSearch = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            // 
            // textBoxFolderPath
            // 
            this.textBoxFolderPath.Location = new System.Drawing.Point(12, 32);
            this.textBoxFolderPath.Name = "textBoxFolderPath";
            this.textBoxFolderPath.ReadOnly = true;
            this.textBoxFolderPath.Size = new System.Drawing.Size(496, 20);
            this.textBoxFolderPath.TabIndex = 0;
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Location = new System.Drawing.Point(514, 30);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowse.TabIndex = 1;
            this.buttonBrowse.Text = "Обзор...";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Выбранная папка";
            // 
            // panelDropTarget
            // 
            this.panelDropTarget.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panelDropTarget.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelDropTarget.Controls.Add(this.label2);
            this.panelDropTarget.Location = new System.Drawing.Point(12, 58);
            this.panelDropTarget.Name = "panelDropTarget";
            this.panelDropTarget.Size = new System.Drawing.Size(577, 60);
            this.panelDropTarget.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(160, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(248, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Перетащите папку сюда или используйте кнопку";
            // 
            // textBoxSearch
            // 
            this.textBoxSearch.Location = new System.Drawing.Point(12, 144);
            this.textBoxSearch.Name = "textBoxSearch";
            this.textBoxSearch.Size = new System.Drawing.Size(400, 20);
            this.textBoxSearch.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 128);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Текст для поиска";
            // 
            // checkBoxTxt
            // 
            this.checkBoxTxt.AutoSize = true;
            this.checkBoxTxt.Checked = true;
            this.checkBoxTxt.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxTxt.Location = new System.Drawing.Point(418, 146);
            this.checkBoxTxt.Name = "checkBoxTxt";
            this.checkBoxTxt.Size = new System.Drawing.Size(46, 17);
            this.checkBoxTxt.TabIndex = 6;
            this.checkBoxTxt.Text = ".txt";
            this.checkBoxTxt.UseVisualStyleBackColor = true;
            this.checkBoxTxt.CheckedChanged += new System.EventHandler(this.checkBoxExtension_CheckedChanged);
            // 
            // checkBoxDocx
            // 
            this.checkBoxDocx.AutoSize = true;
            this.checkBoxDocx.Checked = true;
            this.checkBoxDocx.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxDocx.Location = new System.Drawing.Point(470, 146);
            this.checkBoxDocx.Name = "checkBoxDocx";
            this.checkBoxDocx.Size = new System.Drawing.Size(56, 17);
            this.checkBoxDocx.TabIndex = 7;
            this.checkBoxDocx.Text = ".docx";
            this.checkBoxDocx.UseVisualStyleBackColor = true;
            this.checkBoxDocx.CheckedChanged += new System.EventHandler(this.checkBoxExtension_CheckedChanged);
            // 
            // checkBoxXlsx
            // 
            this.checkBoxXlsx.AutoSize = true;
            this.checkBoxXlsx.Checked = true;
            this.checkBoxXlsx.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxXlsx.Location = new System.Drawing.Point(532, 146);
            this.checkBoxXlsx.Name = "checkBoxXlsx";
            this.checkBoxXlsx.Size = new System.Drawing.Size(50, 17);
            this.checkBoxXlsx.TabIndex = 8;
            this.checkBoxXlsx.Text = ".xlsx";
            this.checkBoxXlsx.UseVisualStyleBackColor = true;
            this.checkBoxXlsx.CheckedChanged += new System.EventHandler(this.checkBoxExtension_CheckedChanged);
            // 
            // buttonSearch
            // 
            this.buttonSearch.Location = new System.Drawing.Point(12, 170);
            this.buttonSearch.Name = "buttonSearch";
            this.buttonSearch.Size = new System.Drawing.Size(75, 23);
            this.buttonSearch.TabIndex = 9;
            this.buttonSearch.Text = "Поиск";
            this.buttonSearch.UseVisualStyleBackColor = true;
            this.buttonSearch.Click += new System.EventHandler(this.buttonSearch_Click);
            // 
            // listBoxResults
            // 
            this.listBoxResults.FormattingEnabled = true;
            this.listBoxResults.HorizontalScrollbar = true;
            this.listBoxResults.Location = new System.Drawing.Point(12, 210);
            this.listBoxResults.Name = "listBoxResults";
            this.listBoxResults.Size = new System.Drawing.Size(577, 225);
            this.listBoxResults.TabIndex = 10;
            this.listBoxResults.DoubleClick += new System.EventHandler(this.listBoxResults_DoubleClick);
            // 
            // progressBarSearch
            // 
            this.progressBarSearch.Location = new System.Drawing.Point(93, 170);
            this.progressBarSearch.Name = "progressBarSearch";
            this.progressBarSearch.Size = new System.Drawing.Size(400, 23);
            this.progressBarSearch.TabIndex = 11;
            // 
            // labelProgress
            // 
            this.labelProgress.AutoSize = true;
            this.labelProgress.Location = new System.Drawing.Point(499, 175);
            this.labelProgress.Name = "labelProgress";
            this.labelProgress.Size = new System.Drawing.Size(21, 13);
            this.labelProgress.TabIndex = 12;
            this.labelProgress.Text = "0%";
            // 
            // labelFileCount
            // 
            this.labelFileCount.AutoSize = true;
            this.labelFileCount.Location = new System.Drawing.Point(12, 445);
            this.labelFileCount.Name = "labelFileCount";
            this.labelFileCount.Size = new System.Drawing.Size(55, 13);
            this.labelFileCount.TabIndex = 13;
            this.labelFileCount.Text = "Файлов: 0";
            // 
            // labelFoundCount
            // 
            this.labelFoundCount.AutoSize = true;
            this.labelFoundCount.Location = new System.Drawing.Point(534, 445);
            this.labelFoundCount.Name = "labelFoundCount";
            this.labelFoundCount.Size = new System.Drawing.Size(55, 13);
            this.labelFoundCount.TabIndex = 14;
            this.labelFoundCount.Text = "Найдено: 0";
            // 
            // backgroundWorkerSearch
            // 
            this.backgroundWorkerSearch.WorkerReportsProgress = true;
            this.backgroundWorkerSearch.WorkerSupportsCancellation = true;
            this.backgroundWorkerSearch.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerSearch_DoWork);
            this.backgroundWorkerSearch.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorkerSearch_ProgressChanged);
            this.backgroundWorkerSearch.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerSearch_RunWorkerCompleted);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(601, 467);
            this.Controls.Add(this.labelFoundCount);
            this.Controls.Add(this.labelFileCount);
            this.Controls.Add(this.labelProgress);
            this.Controls.Add(this.progressBarSearch);
            this.Controls.Add(this.listBoxResults);
            this.Controls.Add(this.buttonSearch);
            this.Controls.Add(this.checkBoxXlsx);
            this.Controls.Add(this.checkBoxDocx);
            this.Controls.Add(this.checkBoxTxt);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxSearch);
            this.Controls.Add(this.panelDropTarget);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonBrowse);
            this.Controls.Add(this.textBoxFolderPath);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Поиск документов";
            this.panelDropTarget.ResumeLayout(false);
            this.panelDropTarget.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxFolderPath;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panelDropTarget;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxSearch;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox checkBoxTxt;
        private System.Windows.Forms.CheckBox checkBoxDocx;
        private System.Windows.Forms.CheckBox checkBoxXlsx;
        private System.Windows.Forms.Button buttonSearch;
        private System.Windows.Forms.ListBox listBoxResults;
        private System.Windows.Forms.ProgressBar progressBarSearch;
        private System.Windows.Forms.Label labelProgress;
        private System.Windows.Forms.Label labelFileCount;
        private System.Windows.Forms.Label labelFoundCount;
        private System.ComponentModel.BackgroundWorker backgroundWorkerSearch;
    }
}