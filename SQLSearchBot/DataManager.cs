using Microsoft.Data.Sqlite;
using System.Collections.Concurrent;
using System.Data;

namespace SQLiteSearchBot
{
    public class DbRecord
    {
        public string DbName { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public int GlobalIndex { get; set; } 
        public Dictionary<string, string> Data { get; set; } = new(); 
    }

    public static class DataManager
    {
        private static readonly ConcurrentDictionary<string, List<DbRecord>> _dataCache = new();
        private static readonly string _userDbPath = "user_state.db";

        public static void LoadData(List<DbConfig> dbConfigs)
        {
            _dataCache.Clear();
            Console.WriteLine("Загрузка данных в память...");

            foreach (var dbConfig in dbConfigs)
            {
                var records = new List<DbRecord>();
                
                try
                {
                    using var connection = new SqliteConnection(dbConfig.ConnectionString);
                    connection.Open();

                    foreach (var tableConfig in dbConfig.Tables)
                    {
                        var command = connection.CreateCommand();
                        command.CommandText = $"SELECT * FROM '{tableConfig.TableName}'";

                        using var reader = command.ExecuteReader();
                        var columns = new List<string>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            columns.Add(reader.GetName(i));
                        }

                        while (reader.Read())
                        {
                            var record = new DbRecord
                            {
                                DbName = dbConfig.Name,
                                TableName = tableConfig.TableName,
                                GlobalIndex = records.Count
                            };

                            foreach (var col in columns)
                            {
                                var val = reader[col];
                                record.Data[col.ToLower()] = val == DBNull.Value ? "" : val.ToString() ?? "";
                            }
                            records.Add(record);
                        }
                    }
                    _dataCache.TryAdd(dbConfig.Name, records);
                    Console.WriteLine($"Загружено {records.Count} записей для БД: {dbConfig.Name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка загрузки БД {dbConfig.Name}: {ex.Message}");
                }
            }
        }

        // --- ЛОГИКА АВТОРИЗАЦИИ ---

        private static void InitUserDb()
        {
            if (!File.Exists(_userDbPath))
            {
                using var connection = new SqliteConnection($"Data Source={_userDbPath}");
                connection.Open();
                var command = connection.CreateCommand();
                // Создаем таблицу с новыми колонками
                command.CommandText = @"
                    CREATE TABLE Users (
                        UserId INTEGER PRIMARY KEY,
                        CurrentDbName TEXT,
                        Access TEXT DEFAULT 'null',
                        Attempts INTEGER DEFAULT 0
                    )";
                command.ExecuteNonQuery();
            }
        }

        // Возвращает статус доступа: 'null' (ждет пароль), 'true' (доступ есть), 'false' (забанен)
        public static string GetUserAccess(long userId)
        {
            InitUserDb();
            using var connection = new SqliteConnection($"Data Source={_userDbPath}");
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT Access FROM Users WHERE UserId = @uid";
            command.Parameters.AddWithValue("@uid", userId);
            
            var result = command.ExecuteScalar();
            return result?.ToString() ?? "null"; // По умолчанию null
        }

        // Проверка пароля. Возвращает: 'success', 'fail', 'banned'
        public static string CheckPassword(long userId, string inputPassword, string correctPassword)
        {
            InitUserDb();
            using var connection = new SqliteConnection($"Data Source={_userDbPath}");
            connection.Open();

            // 1. Проверяем пароль
            if (inputPassword == correctPassword)
            {
                // Пароль верен: сбрасываем попытки, даем доступ
                var updateCmd = connection.CreateCommand();
                updateCmd.CommandText = @"
                    INSERT INTO Users (UserId, Access, Attempts) VALUES (@uid, 'true', 0)
                    ON CONFLICT(UserId) DO UPDATE SET Access='true', Attempts=0";
                updateCmd.Parameters.AddWithValue("@uid", userId);
                updateCmd.ExecuteNonQuery();
                return "success";
            }
            else
            {
                // Пароль неверен: увеличиваем счетчик
                var updateCmd = connection.CreateCommand();
                updateCmd.CommandText = @"
                    INSERT INTO Users (UserId, Access, Attempts) VALUES (@uid, 'null', 1)
                    ON CONFLICT(UserId) DO UPDATE SET Attempts = Attempts + 1";
                updateCmd.Parameters.AddWithValue("@uid", userId);
                updateCmd.ExecuteNonQuery();

                // Проверяем количество попыток
                var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = "SELECT Attempts FROM Users WHERE UserId = @uid";
                checkCmd.Parameters.AddWithValue("@uid", userId);
                var attempts = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (attempts >= 3)
                {
                    // Блокируем
                    var banCmd = connection.CreateCommand();
                    banCmd.CommandText = "UPDATE Users SET Access='false' WHERE UserId = @uid";
                    banCmd.Parameters.AddWithValue("@uid", userId);
                    banCmd.ExecuteNonQuery();
                    return "banned";
                }
                return "fail";
            }
        }

        // Получить текущее число попыток (для вывода сообщения)
        public static int GetAttempts(long userId)
        {
            InitUserDb();
            using var connection = new SqliteConnection($"Data Source={_userDbPath}");
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT Attempts FROM Users WHERE UserId = @uid";
            command.Parameters.AddWithValue("@uid", userId);
            var result = command.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        // Получить выбранную базу (только если доступ есть)
        public static string GetUserActiveDb(long userId)
        {
            InitUserDb();
            using var connection = new SqliteConnection($"Data Source={_userDbPath}");
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT CurrentDbName FROM Users WHERE UserId = @uid AND Access = 'true'";
            command.Parameters.AddWithValue("@uid", userId);
            var result = command.ExecuteScalar();
            return result?.ToString() ?? "";
        }

        public static void SetUserActiveDb(long userId, string dbName)
        {
            InitUserDb();
            using var connection = new SqliteConnection($"Data Source={_userDbPath}");
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Users (UserId, CurrentDbName) VALUES (@uid, @db)
                ON CONFLICT(UserId) DO UPDATE SET CurrentDbName = @db";
            command.Parameters.AddWithValue("@uid", userId);
            command.Parameters.AddWithValue("@db", dbName);
            command.ExecuteNonQuery();
        }
        
        // --- ЛОГИКА ПОИСКА (осталась прежней) ---

        public static List<DbRecord> Search(string dbName, string query)
        {
            if (!_dataCache.ContainsKey(dbName)) return new List<DbRecord>();
            var allRecords = _dataCache[dbName];
            var words = query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(w => w.ToLower()).ToList();

            if (!words.Any()) return new List<DbRecord>();

            var result = allRecords.AsEnumerable();
            foreach (var word in words)
            {
                result = result.Where(r => r.Data.Values.Any(v => v.ToLower().Contains(word)));
            }
            return result.ToList();
        }

        public static DbRecord? GetRecordByIndex(string dbName, int index)
        {
            if (_dataCache.ContainsKey(dbName) && _dataCache[dbName].Count > index)
            {
                return _dataCache[dbName][index];
            }
            return null;
        }
    }
}