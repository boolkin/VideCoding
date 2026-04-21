using JSON_to_SQLite;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHttpClient(); // Важно для работы HTTP
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();