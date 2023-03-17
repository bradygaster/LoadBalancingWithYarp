using System.Diagnostics;
using System.Net;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.LoadBalancing;

var clusters = new[]
{
    new ClusterConfig()
    {
        ClusterId = "servers",
        LoadBalancingPolicy = LoadBalancingPolicies.RoundRobin,
        SessionAffinity = new SessionAffinityConfig
        {
            Enabled = true,
            FailurePolicy = "Redistribute",
            AffinityKeyName = "YARP-SessionAffinity"
        },
        Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
    },
    new ClusterConfig()
    {
        ClusterId = "admin",
        Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
        {
            { "destination1", new DestinationConfig() { Address = "http://localhost:5001" } }
        }
    }
};

var routes = new[]
{
    new RouteConfig()
    {
        RouteId = "admin",
        ClusterId = "admin",
        Match = new RouteMatch {
            Path = "{**catch-all}",
        }
    },
    new RouteConfig()
    {
        RouteId = "servers",
        ClusterId = "servers",
        Match = new RouteMatch {
            Path = "/servers/{**catch-all}",
        }
    }
};

var processName = Process.GetCurrentProcess().ProcessName;
var count = Process.GetProcesses().Count(p => p.ProcessName.Equals(processName)) - 1;

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

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddReverseProxy()
    .UseOrleansConfigProvider(routes, clusters, ConfigureOrleans)
    ;

var app = builder.Build();
app.MapReverseProxy();

app.Run();