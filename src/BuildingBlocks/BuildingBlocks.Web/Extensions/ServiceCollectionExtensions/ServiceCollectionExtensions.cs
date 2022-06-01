using System.Reflection;
using BuildingBlocks.Abstractions.Web;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Web.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection CreatNewServiceCollection(this IServiceCollection services)
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

    public static IServiceCollection AddControllersAsServices(
        this IServiceCollection services,
        params Assembly[] scanAssemblies)
    {
        var assemblies = scanAssemblies.Any() ? scanAssemblies : new[] {Assembly.GetCallingAssembly()};

        return services.Scan(s => s.FromAssemblies(assemblies)
            .AddClasses(f => f.AssignableTo(typeof(ControllerBase)))
            .AsSelf()
            .WithTransientLifetime());
    }
}
