using Microsoft.Data.Sqlite;
using System.Text;
using System.Text.Json;
using System.Linq; 

// ================= КОНФИГУРАЦИЯ =================

class DatabaseConfig 
{ 
    public string file { get; set; } = ""; 
    public List<TableConfig> tables { get; set; } = new(); 
}

class TableConfig 
{ 
    public string name { get; set; } = ""; 
    public List<string> Columns { get; set; } = new(); 
}

class AppConfig 
{ 
    public Limits Limits { get; set; } = new(); 
    public List<DatabaseConfig> Databases { get; set; } = new(); 
}

class Limits 
{ 
    public int MaxSearchLimit { get; set; } = 100; 
    public int MaxResponseSizeBytes { get; set; } = 5242880; 
}

// ================= ДАННЫЕ =================

class MemoryRecord
{
    public string GlobalId { get; set; } = "";
    public string SearchableText { get; set; } = "";
    public string DbAlias { get; set; } = "";
    public string TableName { get; set; } = "";
    
    // Храним все данные
    public Dictionary<string, object?> Data { get; set; } = new();
    
    // ИСПРАВЛЕНИЕ: Сохраняем порядок колонок из конфига, чтобы знать, какая под индексом 1
    public List<string> ColumnNames { get; set; } = new(); 
}

class DataStore
{
    public List<MemoryRecord> AllRecords { get; } = new();
    public Dictionary<string, MemoryRecord> IndexById { get; } = new();
    public int TotalRecordsLoaded { get; private set; }
    
    public List<object> SchemaInfo { get; } = new();

    public void Load(AppConfig config)
    {
        Console.WriteLine("Начинаю загрузку данных в память...");
        foreach (var dbConfig in config.Databases)
        {
            string alias = Path.GetFileNameWithoutExtension(dbConfig.file);
            string dbPath = Path.Combine(AppContext.BaseDirectory, dbConfig.file);

            if (!File.Exists(dbPath))
            {
                Console.WriteLine($"[Warning] Файл БД не найден: {dbPath}. Пропуск.");
                continue;
            }

            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();

            foreach (var tableConfig in dbConfig.tables)
            {
                SchemaInfo.Add(new { db = alias, table = tableConfig.name, columns = tableConfig.Columns });

                if (tableConfig.Columns.Count == 0) continue;

                string columnsSelect = string.Join(", ", tableConfig.Columns);
                string query = $"SELECT {columnsSelect} FROM {tableConfig.name}";

                using var command = new SqliteCommand(query, connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var record = new MemoryRecord
                    {
                        DbAlias = alias,
                        TableName = tableConfig.name
                    };

                    // ИСПРАВЛЕНИЕ: Сохраняем список колонок в запись
                    record.ColumnNames = tableConfig.Columns;

                    var sb = new StringBuilder();
                    
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string colName = reader.GetName(i);
                        var val = reader.GetValue(i);
                        record.Data[colName] = val == DBNull.Value ? null : val;

                        if (i == 0) { }
                        else
                        {
                            if (val != DBNull.Value && val != null)
                            {
                                sb.Append(val.ToString()).Append(" ");
                            }
                        }
                    }

                    string idValue = record.Data[tableConfig.Columns[0]]?.ToString() ?? "unknown";
                    record.GlobalId = $"{alias}:{tableConfig.name}:{idValue}";
                    record.SearchableText = sb.ToString().ToLowerInvariant();

                    AllRecords.Add(record);
                    IndexById[record.GlobalId] = record;
                }
            }
        }
        TotalRecordsLoaded = AllRecords.Count;
        Console.WriteLine($"Загрузка завершена. Всего записей в памяти: {TotalRecordsLoaded}");
    }
}

// ================= ПРОГРАММА =================

class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // === НАЧАЛО ДОБАВЛЕНИЯ CORS ===
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()    // Разрешить запросы с любого сайта
                      .AllowAnyMethod()   // Разрешить GET, POST, DELETE и т.д.
                      .AllowAnyHeader();  // Разрешить любые заголовки
            });
        });
        // === КОНЕЦ ДОБАВЛЕНИЯ CORS ===

        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
        var config = builder.Configuration.Get<AppConfig>()!;

        var dataStore = new DataStore();
        dataStore.Load(config);

        builder.Services.AddSingleton(dataStore);

        var app = builder.Build();
        // === НАЧАЛО ДОБАВЛЕНИЯ MIDDLEWARE ===
        app.UseCors(); 
        // === КОНЕЦ ДОБАВЛЕНИЯ MIDDLEWARE ===
        app.UseExceptionHandler(exceptionHandlerApp =>
        {
            exceptionHandlerApp.Run(async context =>
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                var errorObj = new { error = true, code = "SERVER_ERROR", message = "Внутренняя ошибка сервера." };
                await context.Response.WriteAsJsonAsync(errorObj);
            });
        });

        // 1. Главная / Схема
        app.MapGet("/", () => Results.Ok(new { 
            totalRecords = dataStore.TotalRecordsLoaded, 
            schema = dataStore.SchemaInfo 
        }));

        app.MapGet("/api/schema", () => Results.Ok(new { 
            totalRecords = dataStore.TotalRecordsLoaded, 
            schema = dataStore.SchemaInfo 
        }));

                // 2. Поиск
        app.MapGet("/api/search", (string q, string? fields, string? db, string? table, int? limit) =>
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Results.BadRequest(new { error = true, code = "INVALID_QUERY", message = "Пустой поисковый запрос." });
            }

            var queryLower = q.ToLowerInvariant();

            // === Логика парсинга (И / ИЛИ) ===
            var orGroups = new List<string[]>();
            var matches = System.Text.RegularExpressions.Regex.Matches(queryLower, @"\(([^)]+)\)");
            
            foreach (System.Text.RegularExpressions.Match m in matches)
            {
                string groupContent = m.Groups[1].Value;
                var terms = groupContent.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (terms.Length > 0) orGroups.Add(terms);
            }

            string andQueryPart = System.Text.RegularExpressions.Regex.Replace(queryLower, @"\([^)]+\)", " ");
            var andTerms = andQueryPart.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            // =================================

            IEnumerable<MemoryRecord> queryable = dataStore.AllRecords;

            // Фильтры по БД и Таблицам
            if (!string.IsNullOrWhiteSpace(db))
            {
                var dbList = new HashSet<string>(db.Split(','), StringComparer.OrdinalIgnoreCase);
                queryable = queryable.Where(r => dbList.Contains(r.DbAlias));
            }

            if (!string.IsNullOrWhiteSpace(table))
            {
                var tableList = new HashSet<string>(table.Split(','), StringComparer.OrdinalIgnoreCase);
                queryable = queryable.Where(r => tableList.Contains(r.TableName));
            }

            // Фильтрация поисковым запросу
            var filteredList = queryable
                .AsParallel()
                .Where(r => 
                {
                    bool andMatch = andTerms.All(term => r.SearchableText.Contains(term));
                    if (!andMatch) return false;

                    bool orMatch = orGroups.All(group => group.Any(term => r.SearchableText.Contains(term)));
                    return orMatch;
                })
                .ToList();

            int totalFound = filteredList.Count;

            if (fields == null)
            {
                // --- Режим Разведки ---
                
                // В режиме разведки лимиты обычно не применяются так жестко, 
                // но оставим общую логику безопасности от перегрузки ответа.
                // Если записей очень много, мы просто не показываем распределение по ним,
                // а предлагаем уточнить запрос. Но для простоты вернем все группы, 
                // так как разница в весе между количеством групп и количеством записей огромна.
                // (Если вы хотите ограничить разведку, можно добавить логику здесь).

                var distribution = filteredList
                    .GroupBy(r => $"{r.DbAlias}:{r.TableName}")
                    .Select(g => new { location = g.Key, count = g.Count() })
                    .ToList();

                var result = new { query = q, totalFound = totalFound, distribution = distribution };
                string json = JsonSerializer.Serialize(result);
                string checkedJson = CheckResponseSize(json, config.Limits.MaxResponseSizeBytes);
                
                return checkedJson == json ? Results.Ok(result) : Results.BadRequest(JsonSerializer.Deserialize<object>(checkedJson));
            }
            else
            {
                // --- Режим Данных ---

                IEnumerable<MemoryRecord> recordsToReturnList;
                bool hasMore = false;
                bool isUserLimitSet = limit.HasValue;
                int effectiveLimit = 0;

                // Определяем логику ограничения
                if (!isUserLimitSet)
                {
                    // 1. Лимит НЕ указан пользователем -> используем глобальный из конфига
                    effectiveLimit = config.Limits.MaxRecordsToReturn;

                    if (totalFound > effectiveLimit)
                    {
                        // Превышен глобальный лимит -> ошибка/предупреждение
                        return Results.Ok(new 
                        { 
                            query = q, 
                            count = totalFound,
                            hasMore = true,
                            items = Array.Empty<object>(),
                            message = $"Найдено {totalFound} записей. Это превышает лимит ({effectiveLimit}). Пожалуйста, уточните параметры поиска (db, table) или укажите параметр limit."
                        });
                    }
                    else
                    {
                        // В пределах лимита -> берем все
                        recordsToReturnList = filteredList;
                    }
                }
                else
                {
                    // 2. Лимит УКАЗАН пользователем -> он имеет приоритет
                    effectiveLimit = limit.Value;

                    if (effectiveLimit == 0)
                    {
                        // limit=0 -> вернуть все
                        recordsToReturnList = filteredList;
                        hasMore = false;
                    }
                    else
                    {
                        // limit > 0 -> вернуть последние N записей
                        // Если totalFound (например 8) > effectiveLimit (например 5), берем 5 последних.
                        // Если totalFound < effectiveLimit, TakeLast вернет просто все.
                        recordsToReturnList = filteredList.TakeLast(effectiveLimit);
                        
                        // hasMore должен быть true, если мы что-то отрезали (и лимит не 0)
                        hasMore = (totalFound > effectiveLimit);
                    }
                }

                // Формирование ответа
                var recordsToReturn = recordsToReturnList.Select(r => 
                {
                    object dataContent;

                    if (fields == "*")
                    {
                        dataContent = r.Data;
                    }
                    else if (fields == "1")
                    {
                        if (r.ColumnNames.Count > 1)
                        {
                            string colName = r.ColumnNames[1];
                            r.Data.TryGetValue(colName, out var val);
                            dataContent = new Dictionary<string, object?> { { colName, val } };
                        }
                        else
                        {
                            dataContent = new { }; 
                        }
                    }
                    else if (fields.Trim().Length == 0)
                    {
                        dataContent = new { SearchableText = r.SearchableText };
                    }
                    else
                    {
                        var requestedFields = fields.Split(',').Select(f => f.Trim()).ToList();
                        var dict = new Dictionary<string, object?>();
                        foreach (var f in requestedFields)
                        {
                            if (r.Data.TryGetValue(f, out var val)) dict[f] = val;
                        }
                        dataContent = dict;
                    }

                    return new { globalId = r.GlobalId, data = dataContent };
                }).ToList();

                var resultObj = new { 
                    query = q, 
                    count = totalFound, // Общее количество найденных, независимо от обрезки
                    hasMore = hasMore,
                    items = recordsToReturn 
                };

                string json = JsonSerializer.Serialize(resultObj);
                string checkedJson = CheckResponseSize(json, config.Limits.MaxResponseSizeBytes);
                
                return checkedJson == json ? Results.Ok(resultObj) : Results.BadRequest(JsonSerializer.Deserialize<object>(checkedJson));
            }
        });

        // 3. Batch Get по ID
        app.MapGet("/api/records", (string id) =>
        {
            if (string.IsNullOrWhiteSpace(id)) return Results.BadRequest(new { error = true, code = "INVALID_QUERY", message = "Параметр id обязателен." });

            var ids = id.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ids.Length > 50) return Results.BadRequest(new { error = true, code = "INVALID_QUERY", message = "Максимум 50 ID за запрос." });

            var items = new List<object>();
            var errors = new List<object>();

            foreach (var globalId in ids)
            {
                if (dataStore.IndexById.TryGetValue(globalId, out var record))
                {
                    items.Add(new { globalId = record.GlobalId, data = record.Data });
                }
                else
                {
                    errors.Add(new { id = globalId, message = "Record not found" });
                }
            }

            var result = new { count = items.Count, items, errors };
            string json = JsonSerializer.Serialize(result);
            string checkedJson = CheckResponseSize(json, config.Limits.MaxResponseSizeBytes);

            return checkedJson == json ? Results.Ok(result) : Results.BadRequest(JsonSerializer.Deserialize<object>(checkedJson));
        });

        app.Run();
    }

    static string CheckResponseSize(string json, int maxSizeBytes)
    {
        int size = Encoding.UTF8.GetByteCount(json);
        if (size > maxSizeBytes)
        {
            var errorObj = new { error = true, code = "RESPONSE_TOO_LARGE", message = $"Превышен лимит размера ответа ({size} > {maxSizeBytes})." };
            return JsonSerializer.Serialize(errorObj);
        }
        return json;
    }
}