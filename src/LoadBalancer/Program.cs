using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.LoadBalancing;

var routes = new[]
{
    new RouteConfig()
    {
        RouteId = "servers",
        ClusterId = "servers",
        Match = new RouteMatch {
            Path = "/servers/{**catch-all}",
        }
    }
};

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
,
        },
        Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
        {
            { "destination1", new DestinationConfig() { Address = "http://localhost:5002" } }
        }
    }
};

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .LoadFromMemory(routes, clusters)
    ;

var app = builder.Build();
app.MapReverseProxy();

app.Run();