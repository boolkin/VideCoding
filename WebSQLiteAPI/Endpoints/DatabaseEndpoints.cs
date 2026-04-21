using WebSQLiteAPI.Services;
using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace WebSQLiteAPI.Endpoints;

public static class DatabaseEndpoints
{
    public static void MapDatabaseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/databases");

        // Тот самый эндпоинт из ТЗ
        group.MapGet("/", (DbScannerService scanner) => 
        {
            return Results.Ok(scanner.GetAvailableDatabases());
        });

        group.MapGet("/{dbName}/tables", (string dbName, DbScannerService scanner) => 
        {
            return Results.Ok(scanner.GetTables(dbName));
        });

group.MapGet("/{dbName}/tables/{tableName}", (string dbName, string tableName, int limit = 10, string order = "DESC", string? search = null) => 
{
    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dbName + ".db");
    if (!File.Exists(path)) return Results.NotFound();

    using var connection = new SqliteConnection($"Data Source={path}");
    connection.Open();

    // 1. Получаем список всех колонок для поиска и поиска PK
    var columns = new List<string>();
    string pkColumn = "rowid";
    var schemaCommand = connection.CreateCommand();
    schemaCommand.CommandText = $"PRAGMA table_info(\"{tableName}\")";
    using (var reader = schemaCommand.ExecuteReader()) {
        while (reader.Read()) {
            var colName = reader["name"].ToString();
            columns.Add(colName);
            if (Convert.ToInt32(reader["pk"]) == 1) pkColumn = colName;
        }
    }

    var command = connection.CreateCommand();
    string whereClause = "";

    // 2. Если есть поисковый запрос, формируем условие WHERE col1 LIKE %s% OR col2 LIKE %s%...
    if (!string.IsNullOrWhiteSpace(search))
    {
        var searchParts = columns.Select(c => $"\"{c}\" LIKE @s");
        whereClause = $"WHERE {string.Join(" OR ", searchParts)}";
        command.Parameters.AddWithValue("@s", $"%{search}%");
    }

    command.CommandText = $"SELECT * FROM \"{tableName}\" {whereClause} ORDER BY \"{pkColumn}\" {order} LIMIT @limit";
    command.Parameters.AddWithValue("@limit", limit);
    
    var data = new List<IDictionary<string, object>>();
    using var readerData = command.ExecuteReader();
    while (readerData.Read()) {
        var row = new Dictionary<string, object>();
        for (int i = 0; i < readerData.FieldCount; i++) row.Add(readerData.GetName(i), readerData.GetValue(i));
        data.Add(row);
    }
    return Results.Ok(data);
});

        // Получить структуру колонок
        group.MapGet("/{dbName}/tables/{tableName}/schema", (string dbName, string tableName) => 
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dbName + ".db");
            if (!File.Exists(path)) return Results.NotFound("База данных не найдена");
            var columns = new List<object>();
            using var connection = new SqliteConnection($"Data Source={path}");
            connection.Open();
            
            var command = connection.CreateCommand();
            command.CommandText = $"PRAGMA table_info(\"{tableName}\")";
            
            using var reader = command.ExecuteReader();
            while (reader.Read()) {
                columns.Add(new { 
                    Name = reader["name"].ToString(), 
                    Type = reader["type"].ToString(),
                    Pk = Convert.ToInt32(reader["pk"]) == 1 
                });
            }
            return Results.Ok(columns);
        });

        // Запись данных
       group.MapPost("/{dbName}/tables/{tableName}", async (string dbName, string tableName, HttpRequest request) => 
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dbName + ".db");
            if (!File.Exists(path)) return Results.NotFound("База данных не найдена");
            // Читаем тело как Dictionary<string, JsonElement>
            var jsonBody = await request.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
            if (jsonBody == null) return Results.BadRequest();

            using var connection = new SqliteConnection($"Data Source={path}");
            connection.Open();

            var cols = string.Join(", ", jsonBody.Keys.Select(k => $"\"{k}\""));
            var vars = string.Join(", ", jsonBody.Keys.Select(k => $"@{k}"));
            
            var command = connection.CreateCommand();
            command.CommandText = $"INSERT INTO \"{tableName}\" ({cols}) VALUES ({vars})";
            
            foreach(var entry in jsonBody)
            {
                // РАСПАКОВКА: преобразуем JsonElement в понятный для SQLite тип
                object value = entry.Value.ValueKind switch
                {
                    JsonValueKind.String => entry.Value.GetString(),
                    JsonValueKind.Number => entry.Value.TryGetInt64(out var l) ? l : entry.Value.GetDouble(),
                    JsonValueKind.True => 1,
                    JsonValueKind.False => 0,
                    JsonValueKind.Null => DBNull.Value,
                    _ => entry.Value.GetRawText()
                };
                
                command.Parameters.AddWithValue($"@{entry.Key}", value);
            }

            command.ExecuteNonQuery();
            return Results.Ok(new { success = true });
        });

// Удаление записи
// Мы передаем имя колонки-ключа и её значение в URL
group.MapDelete("/{dbName}/tables/{tableName}/{keyColumn}/{keyValue}", (string dbName, string tableName, string keyColumn, string keyValue) => 
{
    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dbName + ".db");
    if (!File.Exists(path)) return Results.NotFound();

    using var connection = new SqliteConnection($"Data Source={path}");
    connection.Open();

    var command = connection.CreateCommand();
    // Используем параметры для защиты от SQL-инъекций
    command.CommandText = $"DELETE FROM \"{tableName}\" WHERE \"{keyColumn}\" = @val";
    command.Parameters.AddWithValue("@val", keyValue);

    int affected = command.ExecuteNonQuery();
    return affected > 0 ? Results.Ok() : Results.NotFound();
});

group.MapPut("/{dbName}/tables/{tableName}/{keyColumn}/{keyValue}", async (string dbName, string tableName, string keyColumn, string keyValue, HttpRequest request) => 
{
    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dbName + ".db");
    if (!File.Exists(path)) return Results.NotFound("База данных не найдена");
    var jsonBody = await request.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
    
    using var connection = new SqliteConnection($"Data Source={path}");
    connection.Open();

    // Формируем строку SET "col1"=@col1, "col2"=@col2...
    var setClauses = jsonBody.Keys
        .Where(k => k != keyColumn) // Не обновляем сам ID
        .Select(k => $"\"{k}\" = @{k}");

    var command = connection.CreateCommand();
    command.CommandText = $"UPDATE \"{tableName}\" SET {string.Join(", ", setClauses)} WHERE \"{keyColumn}\" = @pkVal";

    foreach(var entry in jsonBody)
    {
        object value = entry.Value.ValueKind switch {
            JsonValueKind.String => entry.Value.GetString(),
            JsonValueKind.Number => entry.Value.TryGetInt64(out var l) ? l : entry.Value.GetDouble(),
            _ => entry.Value.GetRawText()
        };
        command.Parameters.AddWithValue($"@{entry.Key}", value);
    }
    command.Parameters.AddWithValue("@pkVal", keyValue);

    command.ExecuteNonQuery();
    return Results.Ok();
});

group.MapPost("/{dbName}/query", async (string dbName, HttpRequest request) => 
{
    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dbName + ".db");
    if (!File.Exists(path)) return Results.NotFound();

    // Читаем тело запроса (просто строку с SQL)
    using var reader = new StreamReader(request.Body);
    var sql = await reader.ReadToEndAsync();

    var data = new List<IDictionary<string, object>>();
    using var connection = new SqliteConnection($"Data Source={path}");
    connection.Open();

    try {
        var command = connection.CreateCommand();
        command.CommandText = sql;
        
        using var sqlReader = command.ExecuteReader();
        while (sqlReader.Read()) {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < sqlReader.FieldCount; i++) 
                row.Add(sqlReader.GetName(i), sqlReader.GetValue(i));
            data.Add(row);
        }
        return Results.Ok(data);
    }
    catch (Exception ex) {
        return Results.BadRequest(new { error = ex.Message });
    }
});

group.MapPost("/create-db", async (HttpRequest request) => 
{
    var body = await request.ReadFromJsonAsync<Dictionary<string, string>>();
    var dbName = body["dbName"];
    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dbName + ".db");

    if (File.Exists(path)) return Results.BadRequest("Файл уже существует");

    // Создаем пустой файл базы данных
    using (var connection = new SqliteConnection($"Data Source={path}"))
    {
        connection.Open(); // Это автоматически создаст файл
    }
    
    return Results.Ok();
});

    }
}
