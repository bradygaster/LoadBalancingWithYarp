using Yarp.ReverseProxy.Configuration;

public interface IClusterDestinationsGrain : IGrainWithStringKey
{
    public Task AddDestination(string name, DestinationConfig destinationConfig);

    public Task RemoveDestination(string name);

    public Task<Dictionary<string, DestinationConfig>> GetDestinations();

    public Task Subscribe(IClusterDestinationsGrainObserver observer);
}
