using Microsoft.Extensions.Logging;
using Orleans.Utilities;
using Yarp.ReverseProxy.Configuration;

namespace Yarp.Orleans.ConfigProvider.Grains;

public class ClusterDestinationsGrain : IClusterDestinationsGrain
{
    private readonly ILogger<ClusterDestinationsGrain> _logger;
    private readonly Dictionary<string, DestinationConfig> _destinations = new();
    private readonly ObserverManager<IClusterDestinationsGrainObserver> _observers;

    public ClusterDestinationsGrain(ILogger<ClusterDestinationsGrain> logger)
    {
        _logger = logger;
        _observers = new ObserverManager<IClusterDestinationsGrainObserver>(TimeSpan.FromSeconds(60), _logger);
    }

    public Task AddDestination(string name, DestinationConfig destinationConfig)
    {
        if (!_destinations.ContainsKey(name))
        {
            _logger.LogInformation($"Adding {name} to the list of destinations");
            _destinations.Add(name, destinationConfig);
            _observers.Notify(o => o.OnClusterDestinationsChanged(_destinations));
        }
        return Task.CompletedTask;
    }

    public Task RemoveDestination(string name)
    {
        if (_destinations.Remove(name))
        {
            _logger.LogInformation($"Removing {name} from the list of destinations");
            _observers.Notify(o => o.OnClusterDestinationsChanged(_destinations));
        }
        return Task.CompletedTask;
    }

    public Task<Dictionary<string, DestinationConfig>> GetDestinations()
    {
        return Task.FromResult(_destinations);
    }

    public Task Subscribe(IClusterDestinationsGrainObserver observer)
    {
        _observers.Subscribe(observer, observer);
        return Task.CompletedTask;
    }
}
