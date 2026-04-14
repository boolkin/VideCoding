using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http; // Добавлено для работы с запросами
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Threading.Tasks;

namespace OPCWebServer
{
    public class WebService
    {
        private WebApplication? _app;
        private readonly WebSettings _settings;
        private readonly DataPollingService? _pollingService;

        public WebService(WebSettings settings, DataPollingService? pollingService)
        {
            _settings = settings;
            _pollingService = pollingService;
        }

        public async void Start()
        {
            if (!_settings.Enabled) return;

            var builder = WebApplication.CreateBuilder();

            builder.WebHost.UseKestrel(options =>
            {
                options.ListenAnyIP(_settings.Port);
            });

            builder.Services.AddCors(options => options.AddPolicy("AllowAll", 
                p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

            _app = builder.Build();

            _app.UseCors("AllowAll");
            _app.UseDefaultFiles();

            var staticPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _settings.StaticFolder);
            if (!Directory.Exists(staticPath)) Directory.CreateDirectory(staticPath);

            _app.UseStaticFiles(new StaticFileOptions 
            { 
                FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(staticPath)
            });

            // GET эндпоинт для получения тегов
            _app.MapGet("/api/tags", () => 
            {
                var json = _pollingService?.LastJsonData ?? "[]";
                return Results.Content(json, "application/json");
            });

            // POST эндпоинт для сохранения dashboard.json
            _app.MapPost("/api/save", async (HttpRequest request) =>
            {
                try
                {
                    // Путь к файлу в папке статики
                    var filePath = Path.Combine(staticPath, "dashboard.json");

                    // Считываем тело запроса
                    using var reader = new StreamReader(request.Body);
                    var content = await reader.ReadToEndAsync();

                    // Перезаписываем файл
                    await File.WriteAllTextAsync(filePath, content);

                    return Results.Ok(new { message = "Saved successfully" });
                }
                catch (System.Exception ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
            });

            await _app.RunAsync();
        }

        public async void Stop()
        {
            if (_app != null)
            {
                await _app.StopAsync();
                await _app.DisposeAsync();
                _app = null;
            }
        }
    }
}
