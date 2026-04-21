using WebSQLiteAPI.Endpoints;
using WebSQLiteAPI.Services;
using System.Diagnostics; 



var builder = WebApplication.CreateBuilder(args);

// 1. Настройка порта из конфигурации (appsettings.json)
builder.WebHost.UseUrls($"http://*:{builder.Configuration.GetValue<int>("Port", 5000)}");

// 2. Регистрация сервисов
builder.Services.AddSingleton<DbScannerService>();

var app = builder.Build();

// 3. Раздача статики из wwwroot (index.html и прочее)
app.UseDefaultFiles();
app.UseStaticFiles();

// 4. Регистрация API
app.MapDatabaseEndpoints();

Task.Run(() => 
{
    // Небольшая задержка, чтобы сервер успел инициализироваться
    Thread.Sleep(1000); 
    
    var port = builder.Configuration.GetValue<int>("Port", 5000);
    var url = $"http://localhost:{port}/index.html";

    try
    {
        // Универсальный способ открытия браузера для Windows, Linux и macOS
        if (OperatingSystem.IsWindows())
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        else if (OperatingSystem.IsLinux())
        {
            Process.Start("xdg-open", url);
        }
        else if (OperatingSystem.IsMacOS())
        {
            Process.Start("open", url);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Не удалось открыть браузер автоматически: {ex.Message}");
    }
});

app.Run();

public record DatabaseInfo(string Name, string FileName, long Size);