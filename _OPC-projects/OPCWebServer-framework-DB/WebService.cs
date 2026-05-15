using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace OPCWebServer
{
    public class WebService
    {
        private HttpListener _listener;
        private readonly WebSettings _settings;
        private readonly DataPollingService _pollingService;
        private readonly DatabaseService _dbService;
        private bool _isRunning;

        public WebService(WebSettings settings, DataPollingService pollingService, DatabaseService dbService)
        {
            _settings = settings;
            _pollingService = pollingService;
            _dbService = dbService; // Сохраняем ссылку
        }

        public void Start()
        {
            if (!_settings.Enabled) return;

            _listener = new HttpListener();
            // Важно: для прослушивания всех IP нужны права администратора или настройка urlacl
            _listener.Prefixes.Add($"http://*:{_settings.Port}/");
            _listener.Start();
            _isRunning = true;

            Task.Run(() => ListenLoop());
            Console.WriteLine($"Сервер запущен на порту {_settings.Port}");
        }

        private async Task ListenLoop()
        {
            while (_isRunning)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    ProcessRequest(context);
                }
                catch (Exception ex) { /* Логирование */ }
            }
        }

        private async void ProcessRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            // Эмуляция CORS (AllowAll)
            response.AddHeader("Access-Control-Allow-Origin", "*");
            response.AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            response.AddHeader("Access-Control-Allow-Headers", "Content-Type");

            if (request.HttpMethod == "OPTIONS")
            {
                response.StatusCode = (int)HttpStatusCode.OK;
                response.Close();
                return;
            }

            try
            {
                string url = request.Url.AbsolutePath.ToLower();

                // Роут GET /api/tags
                if (url == "/api/tags" && request.HttpMethod == "GET")
                {
                    string json = _pollingService?.LastJsonData ?? "[]";
                    SendResponse(response, json, "application/json");
                }
                // Роут POST /api/save
                else if (url == "/api/save" && request.HttpMethod == "POST")
                {
                    using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        string content = await reader.ReadToEndAsync();
                        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _settings.StaticFolder, "dashboard.json");
                        File.WriteAllText(filePath, content);
                    }
                    SendResponse(response, "{\"message\":\"Saved\"}", "application/json");
                }
                // JsonSerializer.Serialize(data); http://localhost:8085/api/history?id=1&from=2026-05-01T02:40:00&to=2026-05-01T02:45:00&step=2

                else if (url == "/api/history" && request.HttpMethod == "GET")
                {
                    var query = request.QueryString;
                    int.TryParse(query["id"], out int tagId);

                    // Используем временные переменные для парсинга
                    DateTime? fromDate = DateTime.TryParse(query["from"], out var f) ? f : (DateTime?)null;
                    DateTime? toDate = DateTime.TryParse(query["to"], out var t) ? t : (DateTime?)null;

                    int.TryParse(query["step"], out int step);
                    if (step <= 0) step = 1;

                    if (_dbService != null)
                    {
                        var data = _dbService.GetData(tagId, fromDate, toDate, step);
                        string json = JsonSerializer.Serialize(data);
                        SendResponse(response, json, "application/json");
                    }
                    else
                    {
                        SendResponse(response, "{\"error\":\"DB Offline\"}", "application/json");
                    }
                }
                // Роут GET /api/statistics
                else if (url == "/api/statistics" && request.HttpMethod == "GET")
                    {
                        var query = request.QueryString;
                        int.TryParse(query["id"], out int tagId);

                        DateTime? fromDate = DateTime.TryParse(query["from"], out var f) ? f : (DateTime?)null;
                        DateTime? toDate = DateTime.TryParse(query["to"], out var t) ? t : (DateTime?)null;

                        if (_dbService != null)
                        {
                            var data = _dbService.GetStatistics(tagId, fromDate, toDate);
                            string json = JsonSerializer.Serialize(data);
                            SendResponse(response, json, "application/json");
                        }
                        else
                        {
                            SendResponse(response, "{\"error\":\"DB Offline\"}", "application/json");
                        }
                    }

                    // Раздача статики
                    else
                {
                    ServeStaticFile(url, response);
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                SendResponse(response, ex.Message, "text/plain");
            }
        }

        private void ServeStaticFile(string url, HttpListenerResponse response)
        {
            string fileName = url == "/" ? "index.html" : url.TrimStart('/');
            string localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _settings.StaticFolder, fileName);

            if (File.Exists(localPath))
            {
                byte[] buffer = File.ReadAllBytes(localPath);
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.Close();
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Close();
            }
        }

        private void SendResponse(HttpListenerResponse response, string text, string contentType)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            response.ContentType = contentType;
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.Close();
        }

        public void Stop()
        {
            _isRunning = false;
            _listener?.Stop();
            _listener?.Close();
        }
    }
}
