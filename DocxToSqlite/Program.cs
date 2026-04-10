using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Data.Sqlite;

namespace DocxToSqlite
{
    class Program
    {
        static string LogFilePath => Path.Combine(AppContext.BaseDirectory, "processing.log");

        static void WriteLog(string message)
        {
            try { File.AppendAllText(LogFilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}"); } catch { }
        }

        static void Main(string[] args)
        {
            string dbName = "docs_archive";
            string tableName = "documents";
            string? pathInput = null;
            bool isQuiet = false;

            // Убираем лишний "--" из аргументов
            args = args.Where(a => a != "--").ToArray();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "--db": if (i + 1 < args.Length) dbName = args[++i]; break;
                    case "--table": if (i + 1 < args.Length) tableName = args[++i]; break;
                    case "--path": case "-p": if (i + 1 < args.Length) pathInput = args[++i]; break;
                    case "--quiet": case "-q": isQuiet = true; break;
                    case "-h": case "--help":
                        Console.WriteLine("Использование: DocxToSqlite.exe --db <имя> --table <имя> --path <путь> [--quiet]");
                        Console.WriteLine("  --db, -d       Имя файла БД (по умолчанию: docs_archive.db)");
                        Console.WriteLine("  --table, -t    Имя таблицы (по умолчанию: documents)");
                        Console.WriteLine("  --path, -p     Путь к папке с .docx файлами (обязательно в тихом режиме)");
                        Console.WriteLine("  --quiet, -q    Тихий режим для планировщика задач");
                        return;
                }
            }

            if (!dbName.EndsWith(".db", StringComparison.OrdinalIgnoreCase)) dbName += ".db";

            string exeDir = AppContext.BaseDirectory;
            string dbPath = Path.IsPathRooted(dbName) ? dbName : Path.Combine(exeDir, dbName);

            // Логирование запуска
            WriteLog($"ЗАПУСК | Аргументы: {string.Join(" ", args)} | БД: {dbPath} | Таблица: {tableName}");

            // Проверка/создание БД
            if (!File.Exists(dbPath))
            {
                if (isQuiet)
                {
                    WriteLog("Тихий режим: БД не найдена. Создаю автоматически.");
                }
                else
                {
                    Console.WriteLine($"База данных не найдена: {dbPath}");
                    Console.Write("Создать новую базу? (Y/N): ");
                    string? resp = Console.ReadLine()?.Trim().ToLower();
                    if (resp != "y" && resp != "yes")
                    {
                        WriteLog("Запуск отменён пользователем.");
                        Console.WriteLine("Операция отменена.");
                        WaitForKey();
                        return;
                    }
                }
            }

            // Проверка пути
            if (string.IsNullOrWhiteSpace(pathInput))
            {
                if (isQuiet)
                {
                    WriteLog("ОШИБКА: В тихом режиме обязателен аргумент --path <каталог>. Запуск прерван.");
                    Console.WriteLine("Ошибка: В тихом режиме обязательно указывать --path <каталог>");
                    return;
                }
                Console.WriteLine($"Конфигурация: БД={dbPath}, Таблица={tableName}");
                Console.Write("Введите путь к папке для сканирования: ");
                string? rawInput = Console.ReadLine();
                pathInput = rawInput?.Trim().Trim('"') ?? string.Empty;
            }
            else
            {
                pathInput = pathInput.Trim().Trim('"');
            }

            if (!Directory.Exists(pathInput))
            {
                string err = $"Ошибка: Путь '{pathInput}' не найден.";
                WriteLog(err);
                Console.WriteLine(err);
                if (!isQuiet) WaitForKey();
                return;
            }

            Console.WriteLine($"Запуск: БД={dbPath} | Таблица={tableName} | Путь={pathInput} | Тихий режим: {isQuiet}");
            ProcessToDb(dbPath, pathInput, tableName, isQuiet);

            if (!isQuiet) WaitForKey();
        }

        static string GetDocxContent(string filePath)
        {
            try
            {
                using var doc = WordprocessingDocument.Open(filePath, false);
                var body = doc.MainDocumentPart?.Document?.Body;
                if (body == null) return string.Empty;

                var fullText = new List<string>();
                foreach (var element in body.Elements())
                {
                    if (element is Paragraph p)
                    {
                        string text = p.InnerText
                            .Replace("\r\n", " ")
                            .Replace("\n", " ")
                            .Replace("\r", " ")
                            .Trim();
                        if (!string.IsNullOrEmpty(text)) fullText.Add(text);
                    }
                    else if (element is Table table)
                    {
                        foreach (var row in table.Elements<TableRow>())
                        {
                            var cellTexts = row.Elements<TableCell>()
                                .Select(c => c.InnerText.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Trim());
                            fullText.Add("| " + string.Join(" | ", cellTexts) + " |");
                        }
                        fullText.Add(string.Empty);
                    }
                }
                return string.Join("\n", fullText);
            }
            catch (Exception ex) { throw new Exception($"Ошибка чтения файла: {ex.Message}"); }
        }

        static void ProcessToDb(string dbPath, string rootFolder, string tableName, bool isQuiet)
        {
            var allFiles = Directory.EnumerateFiles(rootFolder, "*.docx", SearchOption.AllDirectories)
                                    .Where(f => !Path.GetFileName(f).StartsWith("~$"))
                                    .Select(f => Path.GetFullPath(f))
                                    .ToList();

            int countAdded = 0, countSkipped = 0, countError = 0;
            if (!isQuiet) Console.WriteLine($"\nРаботаем с БД: {dbPath} | Таблица: {tableName}");

            using var conn = new SqliteConnection($"Data Source={dbPath}");
            conn.Open();

            using (var cmd = new SqliteCommand($@"
                CREATE TABLE IF NOT EXISTS [{tableName}] (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    doctext TEXT,
                    filepath TEXT UNIQUE,
                    datetime TEXT
                )", conn))
            {
                cmd.ExecuteNonQuery();
            }

            using var selectCmd = new SqliteCommand($"SELECT 1 FROM [{tableName}] WHERE filepath = @path", conn);
            selectCmd.Parameters.Add("@path", SqliteType.Text);

            using var insertCmd = new SqliteCommand($"INSERT INTO [{tableName}] (doctext, filepath, datetime) VALUES (@text, @path, @modtime)", conn);
            insertCmd.Parameters.Add("@text", SqliteType.Text);
            insertCmd.Parameters.Add("@path", SqliteType.Text);
            insertCmd.Parameters.Add("@modtime", SqliteType.Text);

            SqliteTransaction? currentTx = null;

            try
            {
                for (int i = 0; i < allFiles.Count; i++)
                {
                    string filePath = allFiles[i];
                    try
                    {
                        if (currentTx == null)
                        {
                            currentTx = conn.BeginTransaction();
                            selectCmd.Transaction = currentTx;
                            insertCmd.Transaction = currentTx;
                        }

                        selectCmd.Parameters["@path"].Value = filePath;
                        var exists = selectCmd.ExecuteScalar();

                        if (exists != null)
                        {
                            countSkipped++;
                            if (!isQuiet) UpdateProgress(i + 1, allFiles.Count);
                            continue;
                        }

                        string content = GetDocxContent(filePath);
                        string modTime = File.GetLastWriteTime(filePath).ToString("o");

                        insertCmd.Parameters["@text"].Value = content;
                        insertCmd.Parameters["@path"].Value = filePath;
                        insertCmd.Parameters["@modtime"].Value = modTime;
                        insertCmd.ExecuteNonQuery();

                        countAdded++;

                        if (countAdded % 50 == 0)
                        {
                            currentTx.Commit();
                            currentTx.Dispose();
                            currentTx = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        currentTx?.Rollback();
                        currentTx?.Dispose();
                        currentTx = null;
                        WriteLog($"ОШИБКА ФАЙЛА: {filePath} | {ex.Message}");
                        countError++;
                    }

                    if (!isQuiet) UpdateProgress(i + 1, allFiles.Count);
                }
                currentTx?.Commit();
            }
            finally { currentTx?.Dispose(); }

            string summary = $"Завершено. Новых: {countAdded}, Пропущено: {countSkipped}, Ошибок: {countError}";
            WriteLog(summary);
            Console.WriteLine($"\n{summary}");
        }

        static void UpdateProgress(int current, int total)
        {
            int percent = total > 0 ? (int)((double)current / total * 100) : 100;
            Console.Write($"\rПрогресс: {current}/{total} ({percent}%)");
        }

        static void WaitForKey()
        {
            Console.WriteLine("\nНажмите Enter для выхода...");
            Console.ReadLine();
        }
    }
}