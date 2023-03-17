using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using System.Text.RegularExpressions;
using Yarp.ReverseProxy.Configuration;

public class HealthCheckService : BackgroundService
{
    private readonly IServer _server;
    private readonly IGrainFactory _grainFactory;
    private bool _isHealthy = true;
    private const string CLUSTER_ID = "servers";

    public HealthCheckService(IServer server
        , IGrainFactory grainFactory
        )
    {
        _server = server;
        _grainFactory = grainFactory;
    }

    public async Task SetHealth(bool isHealthy)
    {
        _isHealthy = isHealthy;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var grain = _grainFactory.GetGrain<IClusterDestinationsGrain>(CLUSTER_ID);
        var address = _server.Features.GetRequiredFeature<IServerAddressesFeature>().Addresses.First();
        var name = ExtractName(address);
        
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_isHealthy)
                {
                    await grain.AddDestination(name, new DestinationConfig { Address = address });
                }
                else
                {
                    await grain.RemoveDestination(name);
                }
                await Task.Delay(10_000, stoppingToken);
            }
        }
        catch (TaskCanceledException)
        {
        }
        
        await grain.RemoveDestination(name);
    }

    private static string ExtractName(string uri)
    {
        var match = Regex.Match(uri, "//(?<Hostname>.*):(?<Port>[0-9]+$)");
        if (match.Success)
        {
            return $"{match.Groups["Hostname"]}_{match.Groups["Port"]}";
        }
        return "api";
    }
}