using Yarp.ReverseProxy.Configuration;

public interface IClusterDestinationsGrainObserver : IGrainObserver
{
    void OnClusterDestinationsChanged(Dictionary<string, DestinationConfig> destinations);
}