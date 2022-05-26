using BuildingBlocks.Abstractions.Web;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Web.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection CreatNewCollection(this IServiceCollection services)
    {
        ServiceCollection newServiceCollection = new ServiceCollection();
        foreach (var service in services)
        {
            newServiceCollection.Add(service);
        }

        newServiceCollection.RemoveAll<IHostedService>();

        return newServiceCollection;
    }

    public static IServiceCollection AddGatewayProcessor(this IServiceCollection services)
    {
        services.Replace(ServiceDescriptor.Singleton(typeof(IGatewayProcessor<>), typeof(GatewayProcessor<>)));

        return services;
    }
}
