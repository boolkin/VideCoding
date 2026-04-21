using Microsoft.Data.Sqlite;
using System.Net.Http.Json;
using System.Text.Json; // Обязательно для JsonElement

namespace JSON_to_SQLite;

public class SignalConfig 
{
    public string JsonAddr { get; set; } = string.Empty;
    public string Alias { get; set; } = string.Empty;
}

public class RawSignal
{
    public string addr { get; set; } = string.Empty;
    public object? v { get; set; }
}

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _connectionString;
    private readonly List<SignalConfig> _signals;

    public Worker(ILogger<Worker> logger, IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _config = config;
        _httpClientFactory = httpClientFactory;
        
        // Файл базы будет лежать рядом с EXE
        var dbPath = Path.Combine(AppContext.BaseDirectory, _config["CollectorSettings:DatabaseName"] ?? "data.db");
        _connectionString = $"Data Source={dbPath}";
        
        _signals = _config.GetSection("CollectorSettings:Signals").Get<List<SignalConfig>>() ?? new();
        
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var tableName = _config["CollectorSettings:TableName"] ?? "Metrics";
        
        var createTableSql = $@"
            CREATE TABLE IF NOT EXISTS {tableName} (
                Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                Timestamp DATETIME DEFAULT (datetime('now','localtime'))
            )";
        using (var cmd = new SqliteCommand(createTableSql, connection)) cmd.ExecuteNonQuery();

        foreach (var signal in _signals)
        {
            try
            {
                var addColSql = $"ALTER TABLE {tableName} ADD COLUMN {signal.Alias} TEXT";
                using var cmd = new SqliteCommand(addColSql, connection);
                cmd.ExecuteNonQuery();
            }
            catch { /* Колонка уже есть */ }
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = _config.GetSection("CollectorSettings");
        var interval = settings.GetValue<int>("IntervalSeconds");
        var endpoint = settings["Endpoint"] ?? "";

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(interval));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(5); 

                var response = await client.GetFromJsonAsync<List<RawSignal>>(endpoint, stoppingToken);
                
                if (response != null)
                {
                    await SaveData(response);
                    _logger.LogInformation("Данные записаны в {Db}", _config["CollectorSettings:DatabaseName"]);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка: {Message}", ex.Message);
            }
        }
    }

    private async Task SaveData(List<RawSignal> rawData)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var tableName = _config["CollectorSettings:TableName"] ?? "Metrics";
        var columns = string.Join(", ", _signals.Select(s => s.Alias));
        var parameters = string.Join(", ", _signals.Select(s => "@" + s.Alias));
        
        var sql = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters})";
        
        using var cmd = new SqliteCommand(sql, connection);
        foreach (var sig in _signals)
        {
            var rawValue = rawData.FirstOrDefault(r => r.addr == sig.JsonAddr)?.v;
            
            // Правильная обработка JsonElement и конвертация в строку для БД
            object dbValue = rawValue switch
            {
                JsonElement element => element.ValueKind switch
                {
                    JsonValueKind.String => element.GetString() ?? "",
                    JsonValueKind.Number => element.GetRawText(),
                    JsonValueKind.True => "1",
                    JsonValueKind.False => "0",
                    _ => element.GetRawText()
                },
                null => DBNull.Value,
                _ => rawValue?.ToString() ?? (object)DBNull.Value
            };

            cmd.Parameters.AddWithValue("@" + sig.Alias, dbValue);
        }

        await cmd.ExecuteNonQueryAsync();
    }
}
