using System.Diagnostics;
using System.Net;

var processName = Process.GetCurrentProcess().ProcessName;
var count = Process.GetProcesses().Count(p => p.ProcessName.Equals(processName));
var listeningUri = "http://localhost:" + (3000 + count);
var isHealthy = true;
var timerCounter = 0;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<Server>(new Server(Environment.MachineName, Guid.NewGuid()));

builder.Host.UseOrleans(ConfigureOrleans);

builder.Services
    .AddLogging(logging => logging.SetMinimumLevel(LogLevel.Warning))
    .AddSingleton<HealthCheckService>()
    .AddHostedService(sp => sp.GetRequiredService<HealthCheckService>());

var app = builder.Build();
var healthService = app.Services.GetRequiredService<HealthCheckService>();
var timer = new Timer(Callback, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

app.MapGet("/servers", (Server server) => server);
app.MapGet("/servers/mine", (Server server) => server);

app.Run(listeningUri);

void ConfigureOrleans(ISiloBuilder siloBuilder)
{
    if (count > 0)
    {
        siloBuilder
            .ConfigureEndpoints(siloPort: 11111 + count, gatewayPort: 30000 + count)
            .UseDevelopmentClustering(new IPEndPoint(IPAddress.Loopback, 11111));
    }
    else
    {
        siloBuilder.UseLocalhostClustering();
    }
}

async void Callback(object? state)
{
    if (isHealthy)
    {
        timerCounter++;
        if (timerCounter >= 5)
        {
            timerCounter = 0;
            await healthService.SetHealth(false);
            await Task.Delay(100);
            isHealthy = false;
        }
    }
    else
    {
        await healthService.SetHealth(true);
        isHealthy = true;
    }
}

internal record Server(string MachineName, Guid Identifier) {
    public override string ToString() => $"{MachineName} ({Identifier})";
}
