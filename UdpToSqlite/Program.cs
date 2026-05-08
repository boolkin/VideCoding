using UdpToSqlite;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<UdpWorker>();

var host = builder.Build();
host.Run();
