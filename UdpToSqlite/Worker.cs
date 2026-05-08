namespace UdpToSqlite;

using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using Microsoft.Data.Sqlite;
using System.Text;

public class UdpWorker : BackgroundService
{
    private readonly ILogger<UdpWorker> _logger;
    private readonly IConfiguration _config;
    private readonly Channel<Dictionary<string, object>> _dbQueue;
    private readonly string _connectionString;

    public UdpWorker(ILogger<UdpWorker> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
        _dbQueue = Channel.CreateUnbounded<Dictionary<string, object>>();
        _connectionString = $"Data Source={_config["DatabaseSettings:FileName"]}";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("=== UDP_to_SQLite Service Started ===");
        InitAndSyncDatabase();

        // Фоновая запись в БД
        _ = Task.Run(() => WriteToDbAsync(stoppingToken));

        int port = _config.GetValue<int>("UdpSettings:Port");
        using var udpClient = new UdpClient(port);
        _logger.LogInformation("Listening on UDP port: {Port}", port);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = await udpClient.ReceiveAsync(stoppingToken);
                byte[] buffer = result.Buffer;

                if (_config.GetValue<bool>("UdpSettings:ShowHexInLog"))
                    _logger.LogInformation("HEX: {Hex}", BitConverter.ToString(buffer).Replace("-", " "));

                var data = ParsePacket(buffer);
                
                if (_config.GetValue<bool>("UdpSettings:EnableInfoLogging"))
                    _logger.LogInformation("Received {Bytes} bytes. Mapped {Count} fields.", buffer.Length, data.Count);

                await _dbQueue.Writer.WriteAsync(data, stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Error processing packet");
            }
        }
    }

    private Dictionary<string, object> ParsePacket(byte[] buffer)
    {
        var mappings = _config.GetSection("Mappings").Get<List<MappingItem>>() ?? new();
        var result = new Dictionary<string, object>();
        
        string type = _config["UdpSettings:DataType"]?.ToLower() ?? "float";
        string order = _config["UdpSettings:ByteOrder"]?.ToUpper() ?? "ABCD";
        int size = type switch { "double" => 8, "uint16" => 2, _ => 4 };

        foreach (var map in mappings)
        {
            int offset = map.Index * size;
            if (offset + size <= buffer.Length)
            {
                byte[] raw = buffer.Skip(offset).Take(size).ToArray();
                byte[] ordered = ReorderBytes(raw, order);

                result[map.Alias] = type switch
                {
                    "float"  => BitConverter.ToSingle(ordered, 0),
                    "double" => BitConverter.ToDouble(ordered, 0),
                    "int32"  => BitConverter.ToInt32(ordered, 0),
                    "uint16" => BitConverter.ToUInt16(ordered, 0),
                    _        => 0
                };
            }
        }
        return result;
    }

    private byte[] ReorderBytes(byte[] bytes, string order)
    {
        if (bytes.Length == 4)
        {
            return order switch
            {
                "DCBA" => bytes.Reverse().ToArray(),
                "CDAB" => new byte[] { bytes[1], bytes[0], bytes[3], bytes[2] },
                "BADC" => new byte[] { bytes[2], bytes[3], bytes[0], bytes[1] },
                _ => bytes
            };
        }
        return (bytes.Length == 2 && order == "DCBA") ? bytes.Reverse().ToArray() : bytes;
    }

    private void InitAndSyncDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var tableName = _config["DatabaseSettings:TableName"];
        var mappings = _config.GetSection("Mappings").Get<List<MappingItem>>() ?? new();

        using (var cmd = new SqliteCommand($"CREATE TABLE IF NOT EXISTS {tableName} (Id INTEGER PRIMARY KEY AUTOINCREMENT, Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP)", connection))
            cmd.ExecuteNonQuery();

        var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var cmd = new SqliteCommand($"PRAGMA table_info({tableName})", connection))
        using (var reader = cmd.ExecuteReader())
            while (reader.Read()) existingColumns.Add(reader.GetString(1));

        foreach (var map in mappings)
        {
            if (!existingColumns.Contains(map.Alias))
            {
                string dbType = (_config["UdpSettings:DataType"]?.ToLower().Contains("int") == true) ? "INTEGER" : "REAL";
                using var cmd = new SqliteCommand($"ALTER TABLE {tableName} ADD COLUMN {map.Alias} {dbType}", connection);
                cmd.ExecuteNonQuery();
                _logger.LogInformation("DB Sync: Added column [{Column}]", map.Alias);
            }
        }
    }

    private async Task WriteToDbAsync(CancellationToken ct)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(ct);
        while (await _dbQueue.Reader.WaitToReadAsync(ct))
        {
            while (_dbQueue.Reader.TryRead(out var data))
            {
                try
                {
                    if (data.Count == 0) continue;
                    var cols = string.Join(",", data.Keys);
                    var vals = string.Join(",", data.Keys.Select(k => "@" + k));
                    using var cmd = new SqliteCommand($"INSERT INTO {_config["DatabaseSettings:TableName"]} ({cols}) VALUES ({vals})", connection);
                    foreach (var kv in data) cmd.Parameters.AddWithValue("@" + kv.Key, kv.Value);
                    await cmd.ExecuteNonQueryAsync(ct);
                }
                catch (Exception ex) { _logger.LogError("DB Error: {Msg}", ex.Message); }
            }
        }
    }
}

public class MappingItem { public int Index { get; set; } public string Alias { get; set; } = ""; }
