using Microsoft.Extensions.Hosting;
using Orleans.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yarp.Orleans.ConfigProvider;
using Yarp.ReverseProxy.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class OrleansConfigProviderServiceCollectionExtensions
{
    public static IReverseProxyBuilder UseOrleansConfigProvider(
        this IReverseProxyBuilder builder,
        IReadOnlyList<RouteConfig> routes,
        IReadOnlyList<ClusterConfig> clusters,
        Action<ISiloBuilder> configureOrleans)
    {
        builder.Services
            .AddOrleans(configureOrleans)
            .AddSingleton(sp => ActivatorUtilities.CreateInstance<OrleansConfigProvider>(sp, routes, clusters))
            .AddSingleton<IProxyConfigProvider>(sp => sp.GetRequiredService<OrleansConfigProvider>())
            .AddHostedService(sp => sp.GetRequiredService<OrleansConfigProvider>());
        return builder;
    }
}
