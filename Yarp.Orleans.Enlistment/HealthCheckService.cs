using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using System.Text.RegularExpressions;

public class HealthCheckService : BackgroundService
{
    private readonly IServer _server;
    //private readonly IHttpClientFactory _httpClientBuilder;
    private bool _isHealthy = true;

    public HealthCheckService(IServer server
        //, IHttpClientFactory httpClientBuilder
        )
    {
        _server = server;
        //_httpClientBuilder = httpClientBuilder;
    }

    public async Task SetHealth(bool isHealthy)
    {
        _isHealthy = isHealthy;
        //var client = _httpClientBuilder.CreateClient("HealthCheckService");
        var address = _server.Features.GetRequiredFeature<IServerAddressesFeature>().Addresses.First();
        var name = ExtractName(address);
        var operation = new DestinationOperation
        {
            Address = address,
            Name = name,
            Operation = isHealthy ? Operation.Add : Operation.Remove
        };
        //await client.PostAsJsonAsync("yarp/cluster1", operation);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //var client = _httpClientBuilder.CreateClient("HealthCheckService");
        var address = _server.Features.GetRequiredFeature<IServerAddressesFeature>().Addresses.First();
        var name = ExtractName(address);
        var addMe = new DestinationOperation
        {
            Address = address,
            Name = name,
            Operation = Operation.Add
        };
        var removeMe = new DestinationOperation
        {
            Address = address,
            Name = name,
            Operation = Operation.Remove
        };
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_isHealthy)
                {
                    //await client.PostAsJsonAsync("yarp/cluster1", addMe);
                }
                else
                {
                    //await client.PostAsJsonAsync("yarp/cluster1", removeMe);
                }
                await Task.Delay(10_000, stoppingToken);
            }
        }
        catch (TaskCanceledException)
        {
        }
        //await client.PostAsJsonAsync("yarp/cluster1", removeMe);
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