using Microsoft.Data.Sqlite;

namespace WebSQLiteAPI.Services;

public class DbScannerService
{
    // Метод 1: Сканирование файлов
    public IEnumerable<DatabaseInfo> GetAvailableDatabases()
    {
        var path = AppDomain.CurrentDomain.BaseDirectory;
        var files = Directory.EnumerateFiles(path, "*.*")
            .Where(f => f.EndsWith(".db") || f.EndsWith(".sqlite"));

        foreach (var file in files)
        {
            var info = new FileInfo(file);
            yield return new DatabaseInfo(Path.GetFileNameWithoutExtension(file), info.Name, info.Length);
        }
    }

    // Метод 2: Получение таблиц (убедитесь, что он ВНУТРИ класса)
    public IEnumerable<string> GetTables(string dbName)
    {
        var tables = new List<string>();
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dbName + ".db");

        if (!File.Exists(path)) return tables;

        using var connection = new SqliteConnection($"Data Source={path}");
        connection.Open();
        
        var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'";
        
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            tables.Add(reader.GetString(0));
        }
        return tables;
    }
} 
