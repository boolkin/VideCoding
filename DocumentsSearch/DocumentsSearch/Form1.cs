using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Spreadsheet;

namespace DocumentSearcher
{
    public partial class Form1 : Form
    {
        private string selectedPath = "";
        private List<string> foundFiles = new List<string>();
        private int totalFiles = 0;

        public Form1()
        {
            InitializeComponent();
            InitializeDragDrop();
        }

        private void InitializeDragDrop()
        {
            panelDropTarget.AllowDrop = true;
            panelDropTarget.DragEnter += PanelDropTarget_DragEnter;
            panelDropTarget.DragDrop += PanelDropTarget_DragDrop;
        }

        private void PanelDropTarget_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void PanelDropTarget_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 0)
            {
                string path = files[0];
                if (Directory.Exists(path))
                {
                    selectedPath = path;
                    textBoxFolderPath.Text = path;
                    UpdateFileCount();
                }
            }
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedPath = folderDialog.SelectedPath;
                    textBoxFolderPath.Text = selectedPath;
                    UpdateFileCount();
                }
            }
        }

        private void UpdateFileCount()
        {
            if (string.IsNullOrEmpty(selectedPath) || !Directory.Exists(selectedPath))
            {
                labelFileCount.Text = "Файлов: 0";
                return;
            }

            try
            {
                var extensions = GetSelectedExtensions();
                totalFiles = Directory.GetFiles(selectedPath, "*.*", SearchOption.AllDirectories)
                    .Count(f => extensions.Contains(Path.GetExtension(f).ToLower()));
                labelFileCount.Text = $"Файлов: {totalFiles}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подсчете файлов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<string> GetSelectedExtensions()
        {
            List<string> extensions = new List<string>();

            if (checkBoxTxt.Checked)
                extensions.Add(".txt");
            if (checkBoxDocx.Checked)
                extensions.Add(".docx");
            if (checkBoxXlsx.Checked)
                extensions.Add(".xlsx");

            return extensions;
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            string searchText = textBoxSearch.Text.Trim();
            if (string.IsNullOrEmpty(selectedPath))
            {
                MessageBox.Show("Выберите папку для поиска", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(searchText))
            {
                MessageBox.Show("Введите текст для поиска", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var extensions = GetSelectedExtensions();
            if (extensions.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы одно расширение файлов", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Очистка перед новым поиском
            listBoxResults.Items.Clear();
            foundFiles.Clear();
            progressBarSearch.Value = 0;
            labelProgress.Text = "0%";
            labelFoundCount.Text = "Найдено: 0";

            // Запуск поиска в фоновом потоке
            backgroundWorkerSearch.RunWorkerAsync(new SearchParams
            {
                Path = selectedPath,
                SearchText = searchText,
                Extensions = extensions
            });
        }

        private void backgroundWorkerSearch_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            SearchParams searchParams = e.Argument as SearchParams;

            try
            {
                var files = Directory.GetFiles(searchParams.Path, "*.*", SearchOption.AllDirectories)
                    .Where(f => searchParams.Extensions.Contains(Path.GetExtension(f).ToLower()))
                    .ToArray();

                int processedFiles = 0;
                foundFiles.Clear();

                foreach (string file in files)
                {
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        break;
                    }

                    try
                    {
                        bool found = SearchInFile(file, searchParams.SearchText);
                        if (found)
                        {
                            foundFiles.Add(file);
                            worker.ReportProgress(0, file);
                        }
                    }
                    catch (Exception)
                    {
                        // Игнорируем ошибки чтения отдельных файлов
                    }

                    processedFiles++;
                    int progressPercentage = (int)((double)processedFiles / files.Length * 100);
                    worker.ReportProgress(progressPercentage);
                }
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private bool SearchInFile(string filePath, string searchText)
        {
            string extension = Path.GetExtension(filePath).ToLower();

            switch (extension)
            {
                case ".txt":
                    return SearchInTextFile(filePath, searchText);
                case ".docx":
                    return SearchInWordFile(filePath, searchText);
                case ".xlsx":
                    return SearchInExcelFile(filePath, searchText);
                default:
                    return false;
            }
        }

        private bool SearchInTextFile(string filePath, string searchText)
        {
            try
            {
                // Попробуем разные кодировки
                string[] encodingsToTry = { "utf-8", "windows-1251", "cp866" };

                foreach (string encodingName in encodingsToTry)
                {
                    try
                    {
                        Encoding encoding = Encoding.GetEncoding(encodingName);
                        string content = File.ReadAllText(filePath, encoding);
                        if (content.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            return true;
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                // Если ни одна кодировка не сработала, попробуем автоматическое определение
                string autoContent = File.ReadAllText(filePath, Encoding.Default);
                return autoContent.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch
            {
                return false;
            }
        }

        private bool SearchInWordFile(string filePath, string searchText)
        {
            try
            {
                using (WordprocessingDocument doc = WordprocessingDocument.Open(filePath, false))
                {
                    var body = doc.MainDocumentPart.Document.Body;
                    string text = body.InnerText;
                    return text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool SearchInExcelFile(string filePath, string searchText)
        {
            try
            {
                using (SpreadsheetDocument spreadsheet = SpreadsheetDocument.Open(filePath, false))
                {
                    WorkbookPart workbookPart = spreadsheet.WorkbookPart;

                    // Проверяем shared strings (общие строки)
                    if (workbookPart.SharedStringTablePart != null)
                    {
                        SharedStringTablePart sstpart = workbookPart.SharedStringTablePart;
                        SharedStringTable sst = sstpart.SharedStringTable;

                        foreach (SharedStringItem item in sst.Elements<SharedStringItem>())
                        {
                            if (item.InnerText.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                return true;
                            }
                        }
                    }

                    // Проверяем все листы
                    foreach (WorksheetPart worksheetPart in workbookPart.WorksheetParts)
                    {
                        Worksheet worksheet = worksheetPart.Worksheet;

                        // Проверяем ячейки с inline strings
                        var cellsWithInlineStrings = worksheet.Descendants<Cell>()
                            .Where(c => c.DataType != null && c.DataType.HasValue &&
                                       c.DataType.Value == CellValues.InlineString);

                        foreach (var cell in cellsWithInlineStrings)
                        {
                            var inlineString = cell.GetFirstChild<InlineString>();
                            if (inlineString != null && inlineString.Text != null)
                            {
                                if (inlineString.Text.Text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private void backgroundWorkerSearch_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState != null)
            {
                // Добавление найденного файла в список
                string filePath = e.UserState.ToString();
                listBoxResults.Items.Add(filePath);
            }

            // Обновление прогресса
            progressBarSearch.Value = Math.Min(e.ProgressPercentage, 100);
            labelProgress.Text = $"{e.ProgressPercentage}%";
            labelFoundCount.Text = $"Найдено: {listBoxResults.Items.Count}";
        }

        private void backgroundWorkerSearch_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show($"Ошибка при поиске: {e.Error.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (e.Cancelled)
            {
                MessageBox.Show("Поиск был отменен", "Отмена",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                labelProgress.Text = "100%";
                progressBarSearch.Value = 100;
                labelFoundCount.Text = $"Найдено: {listBoxResults.Items.Count}";
            }
        }

        private void listBoxResults_DoubleClick(object sender, EventArgs e)
        {
            if (listBoxResults.SelectedItem != null)
            {
                string filePath = listBoxResults.SelectedItem.ToString();
                try
                {
                    System.Diagnostics.Process.Start(filePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось открыть файл: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void checkBoxExtension_CheckedChanged(object sender, EventArgs e)
        {
            UpdateFileCount();
        }
    }

    public class SearchParams
    {
        public string Path { get; set; }
        public string SearchText { get; set; }
        public List<string> Extensions { get; set; }
    }
}