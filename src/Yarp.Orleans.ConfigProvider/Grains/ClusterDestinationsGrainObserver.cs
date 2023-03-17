using Yarp.ReverseProxy.Configuration;

namespace Yarp.Orleans.ConfigProvider.Grains;

public sealed class ClusterDestinationsGrainObserver : IClusterDestinationsGrainObserver
{
    private readonly OrleansConfigProvider _configProvider;

    public string ClusterId { get; }

    public ClusterDestinationsGrainObserver(string clusterId, OrleansConfigProvider configProvider)
    {
        ClusterId = clusterId;
        _configProvider = configProvider;
    }

    public void OnClusterDestinationsChanged(Dictionary<string, DestinationConfig> destinations)
    {
        _configProvider.UpdateDestinations(ClusterId, destinations);
    }
}

