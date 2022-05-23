using BuildingBlocks.Abstractions.Web;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

        return newServiceCollection;
    }

    public static IServiceCollection AddGatewayProcessor(this IServiceCollection services)
    {
        services.Replace(ServiceDescriptor.Singleton(typeof(IGatewayProcessor<>), typeof(GatewayProcessor<>)));

        return services;
    }
}
