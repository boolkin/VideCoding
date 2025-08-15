using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using OPC.Common;
using OPC.Data;
using OPC.Data.Interface;

namespace OPCWinFormsClient
{
    public partial class Form1 : Form
    {
        private OpcServerList _serversList;
        private OpcServer _currentServer;
        private List<OpcServers> _availableServers;
        private List<string> _availableTags;
        private List<TagInfo> _selectedTags;
        private List<string> _filteredTags;
        private string _tagsFilePath = "tags.txt";
        private string _loadedConfigPath = ""; // Путь к загруженному файлу конфигурации
        private bool _isUpdatingCoefficients = false; // Флаг для предотвращения рекурсии
        private XmlDocument _configDoc; // Для работы с конфигурацией

        // Класс для хранения информации о теге
        public class TagInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Value { get; set; } = "1";
            public string UpdateRate { get; set; } = "0";
            public string Type { get; set; } = "t";

            public override string ToString()
            {
                return $"{Id}\t{Name}\t{Value}\t{UpdateRate}\t{Type}";
            }

            public static TagInfo FromString(string line)
            {
                var parts = line.Split('\t');
                if (parts.Length >= 5)
                {
                    return new TagInfo
                    {
                        Id = int.Parse(parts[0]),
                        Name = parts[1],
                        Value = parts[2],
                        UpdateRate = parts[3],
                        Type = parts[4]
                    };
                }
                return null;
            }
        }

        public Form1()
        {
            InitializeComponent();
            InitializeOPC();
            _availableTags = new List<string>();
            _selectedTags = new List<TagInfo>();
            _filteredTags = new List<string>();
        }

        private void InitializeOPC()
        {
            try
            {
                _serversList = new OpcServerList();
                LoadAvailableServers();
                CreateDefaultConfiguration(); // Создаем дефолтную конфигурацию
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации OPC: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadAvailableServers()
        {
            try
            {
                OpcServers[] serversArray;
                _serversList.ListAllData20(out serversArray);

                _availableServers = new List<OpcServers>(serversArray);

                listBoxServers.Items.Clear();
                cmbOpcId.Items.Clear();
                foreach (var server in _availableServers)
                {
                    string serverDisplay = $"{server.ProgID}";
                    listBoxServers.Items.Add(serverDisplay);
                    cmbOpcId.Items.Add(serverDisplay);
                }

                UpdateStatus($"Найдено {_availableServers.Count} серверов");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки серверов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void listBoxServers_DoubleClick(object sender, EventArgs e)
        {
            if (listBoxServers.SelectedIndex >= 0)
            {
                LoadServerTags(listBoxServers.SelectedIndex);
            }
        }

        private void LoadServerTags(int serverIndex)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                Application.DoEvents();

                if (_currentServer != null)
                {
                    try
                    {
                        _currentServer.Disconnect();
                    }
                    catch
                    {
                    }
                    _currentServer = null;
                }

                _currentServer = new OpcServer();
                string serverProgID = _availableServers[serverIndex].ProgID;
                _currentServer.Connect(serverProgID);

                ArrayList tagList = new ArrayList();
                _currentServer.Browse(OPCBROWSETYPE.OPC_FLAT, out tagList);

                _availableTags.Clear();
                _filteredTags.Clear();
                listBoxAvailableTags.Items.Clear();
                txtSearch.Text = "";

                foreach (object tag in tagList)
                {
                    string tagString = tag.ToString();
                    _availableTags.Add(tagString);
                    _filteredTags.Add(tagString);
                }

                UpdateAvailableTagsList();
                UpdateStatus($"Загружено {_availableTags.Count} тегов из сервера {serverProgID}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки тегов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("Ошибка загрузки тегов");
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void UpdateAvailableTagsList()
        {
            listBoxAvailableTags.Items.Clear();
            foreach (string tag in _filteredTags)
            {
                listBoxAvailableTags.Items.Add(tag);
            }
        }

        private void listBoxAvailableTags_DoubleClick(object sender, EventArgs e)
        {
            MoveSelectedTagsToRight();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            MoveSelectedTagsToRight();
        }

        private void btnAddAll_Click(object sender, EventArgs e)
        {
            MoveAllTagsToRight();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            MoveSelectedTagsToLeft();
        }

        private void btnRemoveAll_Click(object sender, EventArgs e)
        {
            MoveAllTagsToLeft();
        }

        private void MoveSelectedTagsToRight()
        {
            if (listBoxAvailableTags.SelectedItems.Count == 0) return;

            // Определяем позицию для вставки
            int insertIndex = listBoxSelectedTags.SelectedIndex;
            if (insertIndex == -1)
                insertIndex = _selectedTags.Count; // Если ничего не выделено, добавляем в конец

            List<string> tagsToMove = new List<string>();
            foreach (var item in listBoxAvailableTags.SelectedItems)
            {
                tagsToMove.Add(item.ToString());
            }

            // Вставляем теги в определенную позицию
            int currentIndex = insertIndex;
            foreach (string tagName in tagsToMove)
            {
                string type = DetermineTagType(tagName);

                var tagInfo = new TagInfo
                {
                    Id = 0, // ID будет установлен при перенумерации
                    Name = tagName,
                    Type = type
                };

                _selectedTags.Insert(currentIndex, tagInfo);
                currentIndex++;

                _filteredTags.Remove(tagName);

                int index = listBoxAvailableTags.Items.IndexOf(tagName);
                if (index >= 0)
                {
                    listBoxAvailableTags.Items.RemoveAt(index);
                }
            }

            // Перенумеровываем все теги
            RenumberTags();
            UpdateSelectedTagsList();

            // Выделяем первый добавленный тег
            if (insertIndex < listBoxSelectedTags.Items.Count)
            {
                listBoxSelectedTags.SelectedIndex = insertIndex;
            }
        }

        private string DetermineTagType(string tagName)
        {
            string lowerName = tagName.ToLower();

            if (lowerName.Contains("bool") || lowerName.Contains("bit"))
                return "b";
            else if (lowerName.Contains("int") || lowerName.Contains("word"))
                return "s";
            else
                return "t";
        }

        private void MoveAllTagsToRight()
        {
            // Если есть выделение в правой панели, вставляем после выделенного элемента
            int insertIndex = listBoxSelectedTags.SelectedIndex;
            if (insertIndex == -1)
                insertIndex = _selectedTags.Count;

            int currentIndex = insertIndex;
            foreach (string tagName in new List<string>(_filteredTags))
            {
                string type = DetermineTagType(tagName);

                var tagInfo = new TagInfo
                {
                    Id = 0,
                    Name = tagName,
                    Type = type
                };

                _selectedTags.Insert(currentIndex, tagInfo);
                currentIndex++;
            }

            // Очищаем доступные теги
            _filteredTags.Clear();
            listBoxAvailableTags.Items.Clear();

            // Перенумеровываем все теги
            RenumberTags();
            UpdateSelectedTagsList();
        }

        private void MoveSelectedTagsToLeft()
        {
            if (listBoxSelectedTags.SelectedItems.Count == 0) return;

            List<int> indicesToRemove = new List<int>();
            foreach (int index in listBoxSelectedTags.SelectedIndices)
            {
                indicesToRemove.Add(index);
            }

            // Удаляем в обратном порядке, чтобы индексы не сбивались
            indicesToRemove.Sort((a, b) => b.CompareTo(a));

            foreach (int index in indicesToRemove)
            {
                var tagInfo = _selectedTags[index];

                // Добавляем тег обратно в доступные, если его там нет
                if (!_filteredTags.Contains(tagInfo.Name) && _availableTags.Contains(tagInfo.Name))
                {
                    _filteredTags.Add(tagInfo.Name);
                }

                _selectedTags.RemoveAt(index);
            }

            // Перенумеровываем все теги
            RenumberTags();
            UpdateSelectedTagsList();
            UpdateAvailableTagsList();

            // Очищаем поля редактирования
            ClearCoefficientFields();
        }

        private void MoveAllTagsToLeft()
        {
            // Сохраняем все имена тегов перед очисткой
            var tagNames = new List<string>();
            foreach (var tagInfo in _selectedTags)
            {
                tagNames.Add(tagInfo.Name);
            }

            _selectedTags.Clear();
            listBoxSelectedTags.Items.Clear();

            // Добавляем все теги обратно в доступные
            _filteredTags.Clear();
            foreach (string tag in _availableTags)
            {
                if (DoesTagMatchFilter(tag, txtSearch.Text))
                {
                    _filteredTags.Add(tag);
                }
            }

            UpdateAvailableTagsList();

            // Очищаем поля редактирования
            ClearCoefficientFields();
        }

        private void listBoxSelectedTags_DoubleClick(object sender, EventArgs e)
        {
            MoveSelectedTagsToLeft();
        }

        // Перенумеровывает все теги в правой панели
        private void RenumberTags()
        {
            for (int i = 0; i < _selectedTags.Count; i++)
            {
                _selectedTags[i].Id = i;
            }
        }

        // Обновляет отображение тегов в правой панели
        private void UpdateSelectedTagsList()
        {
            listBoxSelectedTags.Items.Clear();
            foreach (var tagInfo in _selectedTags)
            {
                listBoxSelectedTags.Items.Add(tagInfo.ToString());
            }
        }

        // Перемещение тега вверх
        private void btnMoveUp_Click(object sender, EventArgs e)
        {
            if (listBoxSelectedTags.SelectedIndex <= 0) return;

            int selectedIndex = listBoxSelectedTags.SelectedIndex;

            // Меняем местами теги в списке
            var temp = _selectedTags[selectedIndex];
            _selectedTags[selectedIndex] = _selectedTags[selectedIndex - 1];
            _selectedTags[selectedIndex - 1] = temp;

            // Перенумеровываем теги
            RenumberTags();

            // Обновляем отображение
            UpdateSelectedTagsList();

            // Сохраняем выделение
            listBoxSelectedTags.SelectedIndex = selectedIndex - 1;
        }

        // Перемещение тега вниз
        private void btnMoveDown_Click(object sender, EventArgs e)
        {
            if (listBoxSelectedTags.SelectedIndex == -1 ||
                listBoxSelectedTags.SelectedIndex >= listBoxSelectedTags.Items.Count - 1)
                return;

            int selectedIndex = listBoxSelectedTags.SelectedIndex;

            // Меняем местами теги в списке
            var temp = _selectedTags[selectedIndex];
            _selectedTags[selectedIndex] = _selectedTags[selectedIndex + 1];
            _selectedTags[selectedIndex + 1] = temp;

            // Перенумеровываем теги
            RenumberTags();

            // Обновляем отображение
            UpdateSelectedTagsList();

            // Сохраняем выделение
            listBoxSelectedTags.SelectedIndex = selectedIndex + 1;
        }

        // Обработчик выбора тега в правой панели
        private void listBoxSelectedTags_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isUpdatingCoefficients) return; // Предотвращаем рекурсию

            if (listBoxSelectedTags.SelectedIndex >= 0 && listBoxSelectedTags.SelectedIndex < _selectedTags.Count)
            {
                var selectedTag = _selectedTags[listBoxSelectedTags.SelectedIndex];
                LoadTagCoefficients(selectedTag);
            }
            else
            {
                ClearCoefficientFields();
            }
        }

        // Загрузка коэффициентов тега в поля редактирования
        private void LoadTagCoefficients(TagInfo tag)
        {
            _isUpdatingCoefficients = true;
            try
            {
                txtValue.Text = tag.Value;
                txtUpdateRate.Text = tag.UpdateRate;
                cmbType.Text = tag.Type;
            }
            finally
            {
                _isUpdatingCoefficients = false;
            }
        }

        // Очистка полей редактирования
        private void ClearCoefficientFields()
        {
            _isUpdatingCoefficients = true;
            try
            {
                txtValue.Text = "";
                txtUpdateRate.Text = "";
                cmbType.Text = "";
            }
            finally
            {
                _isUpdatingCoefficients = false;
            }
        }

        // Обработчики изменений в полях редактирования
        private void txtValue_TextChanged(object sender, EventArgs e)
        {
            if (_isUpdatingCoefficients) return;

            if (listBoxSelectedTags.SelectedIndex >= 0 && listBoxSelectedTags.SelectedIndex < _selectedTags.Count)
            {
                _selectedTags[listBoxSelectedTags.SelectedIndex].Value = txtValue.Text;
                UpdateSelectedTagDisplay();
            }
        }

        private void txtUpdateRate_TextChanged(object sender, EventArgs e)
        {
            if (_isUpdatingCoefficients) return;

            if (listBoxSelectedTags.SelectedIndex >= 0 && listBoxSelectedTags.SelectedIndex < _selectedTags.Count)
            {
                _selectedTags[listBoxSelectedTags.SelectedIndex].UpdateRate = txtUpdateRate.Text;
                UpdateSelectedTagDisplay();
            }
        }

        private void cmbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isUpdatingCoefficients) return;

            if (listBoxSelectedTags.SelectedIndex >= 0 && listBoxSelectedTags.SelectedIndex < _selectedTags.Count)
            {
                _selectedTags[listBoxSelectedTags.SelectedIndex].Type = cmbType.Text;
                UpdateSelectedTagDisplay();
            }
        }

        // Обновление отображения выбранного тега
        private void UpdateSelectedTagDisplay()
        {
            if (listBoxSelectedTags.SelectedIndex >= 0 && listBoxSelectedTags.SelectedIndex < _selectedTags.Count)
            {
                _isUpdatingCoefficients = true;
                try
                {
                    var tag = _selectedTags[listBoxSelectedTags.SelectedIndex];
                    string displayText = tag.ToString();

                    // Сохраняем текущую позицию прокрутки
                    int topIndex = listBoxSelectedTags.TopIndex;

                    // Обновляем элемент в списке
                    listBoxSelectedTags.Items[listBoxSelectedTags.SelectedIndex] = displayText;

                    // Восстанавливаем позицию прокрутки
                    listBoxSelectedTags.TopIndex = topIndex;
                }
                finally
                {
                    _isUpdatingCoefficients = false;
                }
            }
        }

        private void UpdateStatus(string message)
        {
            toolStripStatusLabel.Text = message;
        }

        private void btnRefreshServers_Click(object sender, EventArgs e)
        {
            LoadAvailableServers();
            UpdateStatus("Серверы обновлены");
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            FilterTags();
        }

        private void FilterTags()
        {
            string searchText = txtSearch.Text.Trim();

            _filteredTags.Clear();

            if (string.IsNullOrEmpty(searchText))
            {
                _filteredTags.AddRange(_availableTags);
            }
            else
            {
                foreach (string tag in _availableTags)
                {
                    if (DoesTagMatchFilter(tag, searchText))
                    {
                        _filteredTags.Add(tag);
                    }
                }
            }

            UpdateAvailableTagsList();
            UpdateStatus($"Найдено {_filteredTags.Count} тегов");
        }

        private bool DoesTagMatchFilter(string tag, string filter)
        {
            if (string.IsNullOrEmpty(filter))
                return true;

            if (filter.Contains("*"))
            {
                string pattern = filter.Replace("*", ".*");
                try
                {
                    var regex = new System.Text.RegularExpressions.Regex(pattern,
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    return regex.IsMatch(tag);
                }
                catch
                {
                    return tag.IndexOf(filter.Replace("*", ""), StringComparison.OrdinalIgnoreCase) >= 0;
                }
            }
            else
            {
                return tag.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
            }
        }

        private void btnImportTags_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                    Title = "Выберите файл с тегами"
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string[] lines = File.ReadAllLines(openFileDialog.FileName);

                    // Если есть выделение в правой панели, вставляем после него
                    int insertIndex = listBoxSelectedTags.SelectedIndex;
                    if (insertIndex == -1)
                        insertIndex = _selectedTags.Count;

                    int currentIndex = insertIndex;
                    foreach (string line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            var tagInfo = TagInfo.FromString(line.Trim());
                            if (tagInfo != null)
                            {
                                _selectedTags.Insert(currentIndex, tagInfo);
                                currentIndex++;
                            }
                        }
                    }

                    // Перенумеровываем все теги
                    RenumberTags();
                    UpdateSelectedTagsList();

                    UpdateStatus($"Импортировано тегов из файла {openFileDialog.FileName}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка импорта: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExportTags_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                    Title = "Сохранить теги в файл",
                    FileName = "tags.txt"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    List<string> lines = new List<string>();
                    foreach (var tagInfo in _selectedTags)
                    {
                        lines.Add(tagInfo.ToString());
                    }

                    File.WriteAllLines(saveFileDialog.FileName, lines);
                    UpdateStatus($"Экспортировано {_selectedTags.Count} тегов в файл {saveFileDialog.FileName}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Загрузка конфигурации - открывает диалог выбора файла и загружает конфигурацию
        private void btnLoadConfig_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Config files (*.config)|*.config|All files (*.*)|*.*",
                    Title = "Выберите файл конфигурации"
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Загружаем конфигурацию из выбранного файла
                    _configDoc = new XmlDocument();
                    _configDoc.Load(openFileDialog.FileName);
                    _loadedConfigPath = openFileDialog.FileName; // Сохраняем путь к файлу

                    // Загружаем значения в поля формы
                    LoadConfigToForm();
                    UpdateStatus($"Конфигурация загружена из файла {openFileDialog.FileName}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки конфигурации: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Экспорт конфигурации - сохраняет текущую конфигурацию в файл
        private void btnExportConfig_Click(object sender, EventArgs e)
        {
            try
            {
                string fileNameToSave = "OPCDA_to_JSON.exe.config";

                // Если был загружен файл конфигурации, используем его имя
                if (!string.IsNullOrEmpty(_loadedConfigPath))
                {
                    fileNameToSave = Path.GetFileName(_loadedConfigPath);
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Config files (*.config)|*.config|All files (*.*)|*.*",
                    Title = "Сохранить конфигурацию в файл",
                    FileName = fileNameToSave
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Сохраняем текущую конфигурацию
                    SaveFormToConfig();
                    if (_configDoc != null)
                    {
                        _configDoc.Save(saveFileDialog.FileName);
                        UpdateStatus($"Конфигурация экспортирована в файл {saveFileDialog.FileName}");
                    }
                    else
                    {
                        MessageBox.Show("Конфигурация не загружена", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта конфигурации: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Загрузка конфигурации в форму
        private void LoadConfigToForm()
        {
            try
            {
                if (_configDoc == null) return;

                // Загружаем значения из конфигурации в поля формы
                LoadConfigValueToControl("opcID", cmbOpcId);
                LoadConfigValueToControl("refreshTime", txtRefreshTime);
                LoadConfigValueToControl("consoleOutput", cmbConsoleOutput);
                LoadConfigValueToControl("webServer", cmbWebServer);
                LoadConfigValueToControl("portNumber", txtPortNumber);
                LoadConfigValueToControl("udpSend", cmbUdpSend);
                LoadConfigValueToControl("remotePort", txtRemotePort);
                LoadConfigValueToControl("remoteIP", txtRemoteIP);
                LoadConfigValueToControl("tags2send", txtTags2Send);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки конфигурации в форму: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Сохранение формы в конфигурацию
        private void SaveFormToConfig()
        {
            try
            {
                if (_configDoc == null)
                {
                    CreateDefaultConfiguration();
                }

                // Сохраняем значения из полей формы в конфигурацию
                SaveControlValueToConfig("opcID", cmbOpcId.Text);
                SaveControlValueToConfig("refreshTime", txtRefreshTime.Text);
                SaveControlValueToConfig("consoleOutput", cmbConsoleOutput.Text);
                SaveControlValueToConfig("webServer", cmbWebServer.Text);
                SaveControlValueToConfig("portNumber", txtPortNumber.Text);
                SaveControlValueToConfig("udpSend", cmbUdpSend.Text);
                SaveControlValueToConfig("remotePort", txtRemotePort.Text);
                SaveControlValueToConfig("remoteIP", txtRemoteIP.Text);
                SaveControlValueToConfig("tags2send", txtTags2Send.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения формы в конфигурацию: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Загрузка значения из конфигурации в контрол
        private void LoadConfigValueToControl(string key, Control control)
        {
            try
            {
                XmlNode node = _configDoc.SelectSingleNode($"//appSettings/add[@key='{key}']");
                if (node != null)
                {
                    string value = node.Attributes["value"].Value;
                    if (control is TextBox)
                    {
                        ((TextBox)control).Text = value;
                    }
                    else if (control is ComboBox)
                    {
                        ((ComboBox)control).Text = value;
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки загрузки отдельных значений
            }
        }

        // Сохранение значения из контрола в конфигурацию
        private void SaveControlValueToConfig(string key, string value)
        {
            try
            {
                if (_configDoc == null) return;

                XmlNode node = _configDoc.SelectSingleNode($"//appSettings/add[@key='{key}']");
                if (node != null)
                {
                    node.Attributes["value"].Value = value;
                }
            }
            catch
            {
                // Игнорируем ошибки сохранения отдельных значений
            }
        }

        // Создание дефолтной конфигурации
        private void CreateDefaultConfiguration()
        {
            _configDoc = new XmlDocument();

            string defaultConfig = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <startup>
    <supportedRuntime version=""v4.0"" sku="".NETFramework,Version=v4.6.1"" />
  </startup>
  <appSettings>
    <!-- OPC settings -->
    <add key=""opcID"" value=""Graybox.Simulator"" />
    <add key=""refreshTime"" value=""700"" />
    <!-- Console settings. ""yes"" to show values in console-->
    <add key=""consoleOutput"" value=""yes"" />
    <!-- Web server settings -->
    <add key=""webServer"" value=""yes"" />
    <add key=""portNumber"" value=""45455"" />
    <!-- UDP settings -->
    <add key=""udpSend"" value=""yes"" />
    <add key=""remotePort"" value=""3310"" />
    <add key=""remoteIP"" value=""127.0.0.1"" />
    <add key=""tags2send"" value=""2"" />
    <add key=""ClientSettingsProvider.ServiceUri"" value="""" />
  </appSettings>
</configuration>";

            _configDoc.LoadXml(defaultConfig);

            // Инициализация комбобоксов дефолтными значениями
            cmbConsoleOutput.Text = "yes";
            cmbWebServer.Text = "yes";
            cmbUdpSend.Text = "yes";
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Инициализация комбобоксов
            cmbType.Items.Clear();
            cmbType.Items.AddRange(new string[] { "b", "s", "!b", "t" });

            cmbConsoleOutput.Items.Clear();
            cmbConsoleOutput.Items.AddRange(new string[] { "yes", "no" });

            cmbWebServer.Items.Clear();
            cmbWebServer.Items.AddRange(new string[] { "yes", "no" });

            cmbUdpSend.Items.Clear();
            cmbUdpSend.Items.AddRange(new string[] { "yes", "no" });

            if (File.Exists(_tagsFilePath))
            {
                try
                {
                    string[] lines = File.ReadAllLines(_tagsFilePath);

                    foreach (string line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            var tagInfo = TagInfo.FromString(line.Trim());
                            if (tagInfo != null)
                            {
                                _selectedTags.Add(tagInfo);
                            }
                        }
                    }

                    // Перенумеровываем теги при загрузке
                    RenumberTags();
                    UpdateSelectedTagsList();
                    UpdateStatus($"Загружено {_selectedTags.Count} тегов из файла {_tagsFilePath}");
                }
                catch (Exception ex)
                {
                    UpdateStatus($"Ошибка загрузки тегов: {ex.Message}");
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Сохраняем конфигурацию перед закрытием
            SaveFormToConfig();
            base.OnFormClosing(e);
        }
    }
}