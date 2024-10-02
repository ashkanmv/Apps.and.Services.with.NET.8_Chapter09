using Northwind.Background.Workers;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<TimerWorker>();

var host = builder.Build();
host.Run();
