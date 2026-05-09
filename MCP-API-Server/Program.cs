using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
builder.Services.AddCors();
var app = builder.Build();

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

// Загружаем настройки
var mcpConfig = app.Configuration.GetSection("McpConfig");
var apiBase = mcpConfig["ApiBase"];
var toolsConfig = mcpConfig.GetSection("Tools").Get<List<ToolDefinition>>();

string? GetJsonString(JsonElement element) => element.ValueKind switch
{
    JsonValueKind.String => element.GetString(),
    JsonValueKind.Number => element.GetRawText(),
    JsonValueKind.True => "true",
    JsonValueKind.False => "false",
    _ => null
};

app.MapPost("/mcp", async (HttpContext context, [FromBody] JsonElement request, IHttpClientFactory clientFactory) =>
{
    if (!request.TryGetProperty("id", out var idProp)) return Results.Content("{}", "application/json");

    var method = request.GetProperty("method").GetString();
    var requestId = idProp.GetRawText();

    // 1. Листинг инструментов из конфига
if (method == "tools/list")
{
    var responseTools = toolsConfig.Select(t => {
        // Собираем свойства для схемы на основе PassThroughParams и Mapping
        var properties = new Dictionary<string, object>();
        
        if (t.PassThroughParams != null)
            foreach (var p in t.PassThroughParams) 
                properties[p] = new { type = "string" };

        if (t.Mapping != null)
            foreach (var m in t.Mapping) 
                properties[m.Key] = new { type = "string" };

        return new {
            name = t.Name,
            description = t.Description,
            inputSchema = new {
                type = "object",
                properties = properties,
                required = t.PassThroughParams ?? new List<string>()
            }
        };
    });

    return Results.Json(new {
        jsonrpc = "2.0",
        id = requestId,
        result = new { tools = responseTools }
    });
}


   if (method == "tools/call")
{
    var toolParams = request.GetProperty("params");
    var toolName = toolParams.GetProperty("name").GetString();
    var args = toolParams.GetProperty("arguments");

    // ЛОГ: Показывает, что именно прислала LLM
    Console.WriteLine($"[Incoming MCP] Args: {args.GetRawText()}");

    var tool = toolsConfig.FirstOrDefault(t => t.Name == toolName);
    if (tool == null) return Results.Json(new { jsonrpc = "2.0", id = requestId, error = new { message = "Tool not found" } });

    var query = new Dictionary<string, string?>();

    // 1. Фиксированные параметры
    if (tool.FixedParams != null)
        foreach (var p in tool.FixedParams) query[p.Key] = p.Value;

    // 2. Универсальный поиск параметров (Case-Insensitive)
    // Превращаем параметры от LLM в словарь для удобного поиска
    var argsDict = args.EnumerateObject()
                       .ToDictionary(p => p.Name.ToLower(), p => p.Value);

    // Обработка Mapping (MCP -> API)
    if (tool.Mapping != null)
    {
        foreach (var m in tool.Mapping)
        {
            if (argsDict.TryGetValue(m.Key.ToLower(), out var val))
                query[m.Value] = GetJsonString(val);
        }
    }

    // Обработка PassThrough (q, db, table)
    if (tool.PassThroughParams != null)
    {
        foreach (var p in tool.PassThroughParams)
        {
            if (argsDict.TryGetValue(p.ToLower(), out var val))
                query[p] = GetJsonString(val);
        }
    }

    var finalUrl = QueryHelpers.AddQueryString($"{apiBase.TrimEnd('/')}{tool.Endpoint}", query);
    Console.WriteLine($"[MCP Call] Tool: {toolName} -> URL: {finalUrl}");

        var url = QueryHelpers.AddQueryString($"{apiBase.TrimEnd('/')}{tool.Endpoint}", query);
        
        try {
            var client = clientFactory.CreateClient();
            var response = await client.GetStringAsync(url);
            return Results.Json(new {
                jsonrpc = "2.0", id = requestId,
                result = new { content = new[] { new { type = "text", text = response } } }
            });
        } catch (Exception ex) {
            return Results.Json(new { jsonrpc = "2.0", id = requestId, result = new { content = new[] { new { type = "text", text = $"API Error: {ex.Message}" } }, isError = true } });
        }
    }

    // Обработка инициализации (стандартно)
    if (method == "initialize") return Results.Json(new { jsonrpc = "2.0", id = requestId, result = new { protocolVersion = "2024-11-05", capabilities = new { tools = new { } }, serverInfo = new { name = "MCP-API-Server", version = "1.0" } } });

    return Results.Json(new { jsonrpc = "2.0", id = requestId, error = new { code = -32601, message = "Method not found" } });
});

// Получаем порт из конфига, если его нет — используем 8001 по умолчанию
var port = builder.Configuration.GetValue<int>("McpConfig:Port", 8001);

// Запускаем на всех интерфейсах (0.0.0.0) или только на локальном (127.0.0.1)
app.Run($"http://127.0.0.1:{port}");

// Модели для десериализации конфига
public class ToolDefinition {
    public string Name { get; set; }
    public string Description { get; set; }
    public string Endpoint { get; set; }
    public Dictionary<string, string>? FixedParams { get; set; }
    public Dictionary<string, string>? Mapping { get; set; }
    public List<string>? PassThroughParams { get; set; }
}
