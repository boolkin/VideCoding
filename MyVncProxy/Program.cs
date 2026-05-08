using WebsockifySharp;

var builder = WebApplication.CreateBuilder(args);

// Настройка порта сервера
var httpPort = builder.Configuration.GetValue<int>("ServerSettings:HttpPort");
builder.WebHost.ConfigureKestrel(options => options.ListenAnyIP(httpPort));

var app = builder.Build();

// Важно: StaticFiles должны быть ДО маппинга API и прокси
app.UseDefaultFiles(); 
app.UseStaticFiles();
app.UseWebSockets();

// 1. Получаем список серверов для настройки прокси
var vncSections = builder.Configuration.GetSection("VncServers").GetChildren().ToList();

foreach (var server in vncSections)
{
    var name = server["Name"];
    var host = server["Host"];
    var port = int.Parse(server["Port"] ?? "5900");

    // Создаем маршруты прокси
    app.UseWebsockify($"/vnc/{name}", host, port);
}

// 2. API для получения списка серверов фронтендом
app.MapGet("/api/servers", () => {
    return vncSections.Select(s => new {
        Name = s["Name"],
        Host = s["Host"],
        Port = s["Port"],
        Password = s["Password"] // Добавляем это поле
    });
});


Console.WriteLine($"Управление запущено на http://localhost:{httpPort}");
app.Run();
