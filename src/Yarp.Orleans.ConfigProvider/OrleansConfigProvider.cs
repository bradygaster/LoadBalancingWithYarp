using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Data;
using Yarp.Orleans.ConfigProvider.Grains;
using Yarp.ReverseProxy.Configuration;

namespace Yarp.Orleans.ConfigProvider;

public sealed class OrleansConfigProvider : BackgroundService, IProxyConfigProvider
{
    private readonly IClusterClient _client;
    private readonly ILogger<OrleansConfigProvider> _logger;
    private readonly Dictionary<string, (IClusterDestinationsGrainObserver Observer, IClusterDestinationsGrainObserver Reference)> _observers;
    private readonly InMemoryConfigProvider _memoryConfigProvider;

    public OrleansConfigProvider(
        IClusterClient client,
        IReadOnlyList<RouteConfig> routes,
        IReadOnlyList<ClusterConfig> clusters,
        ILogger<OrleansConfigProvider> logger)
    {
        if (routes == default) throw new ArgumentNullException(nameof(routes));
        _client = client;
        _logger = logger;
        _observers = new();
        _memoryConfigProvider = new InMemoryConfigProvider(routes, clusters);
        foreach (var cluster in clusters)
        {
            var obs = new ClusterDestinationsGrainObserver(cluster.ClusterId, this);
            _observers.Add(
                cluster.ClusterId,
                (obs, _client.CreateObjectReference<IClusterDestinationsGrainObserver>(obs)));
        }
    }

    public IProxyConfig GetConfig() => _memoryConfigProvider.GetConfig();

    public void UpdateDestinations(string clusterId, Dictionary<string, DestinationConfig> destinations)
    {
        _logger.LogInformation("Updating destination for {ClusterId}: {Destinations}", clusterId, destinations.Select(d => d.Key));
        var oldConfig = GetConfig();
        var clusterConfigs = new List<ClusterConfig>(oldConfig.Clusters.Count);
        foreach (var orig in oldConfig.Clusters)
        {
            var newDestinations = orig.ClusterId.Equals(clusterId)
                ? destinations
                : orig.Destinations;
            var cfg = new ClusterConfig()
            {
                ClusterId = orig.ClusterId,
                HealthCheck = orig.HealthCheck,
                HttpClient = orig.HttpClient,
                HttpRequest = orig.HttpRequest,
                LoadBalancingPolicy = orig.LoadBalancingPolicy,
                Metadata = orig.Metadata,
                SessionAffinity = orig.SessionAffinity,
                Destinations = newDestinations,
            };
            clusterConfigs.Add(cfg);
        }
        _memoryConfigProvider.Update(oldConfig.Routes, clusterConfigs);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var observer in _observers)
                {
                    var grain = _client.GetGrain<IClusterDestinationsGrain>(observer.Key);
                    await grain.Subscribe(observer.Value.Reference);
                }
                await Task.Delay(10_000, stoppingToken);
            }
        }
        catch (TaskCanceledException)
        {
            // Ignore
        }
    }
}
