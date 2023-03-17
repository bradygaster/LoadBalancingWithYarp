using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yarp.ReverseProxy.Configuration;

[GenerateSerializer]
public struct DestinationConfigSurrogate
{
    [Id(0)]
    public string Address { get; init; }

    [Id(1)]
    public string? Health { get; init; }

    [Id(2)]
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }
}

[RegisterConverter]
public sealed class DestinationConfigSurrogateConverter : IConverter<DestinationConfig, DestinationConfigSurrogate>
{
    public DestinationConfig ConvertFromSurrogate(in DestinationConfigSurrogate surrogate) => new()
    {
        Address = surrogate.Address,
        Health = surrogate.Health,
        Metadata = surrogate.Metadata
    };

    public DestinationConfigSurrogate ConvertToSurrogate(in DestinationConfig value) => new()
    {
        Address = value.Address,
        Health = value.Health,
        Metadata = value.Metadata
    };
}
