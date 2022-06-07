using System.Reflection;
using BuildingBlocks.Abstractions.Core;
using BuildingBlocks.Abstractions.CQRS.Event;
using BuildingBlocks.Abstractions.Serialization;
using BuildingBlocks.Abstractions.Types;
using BuildingBlocks.Core.CQRS.Event;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Core.IdsGenerator;
using BuildingBlocks.Core.Serialization;
using BuildingBlocks.Core.Types;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Core.Registrations;

public static class CoreRegistrationExtensions
{
    public static IServiceCollection AddCore(
        this IServiceCollection services,
        IConfiguration configuration,
        string? rootSectionName = null,
        params Assembly[] assembliesToScan)
    {
        var systemInfo = MachineInstanceInfo.New();

        services.AddSingleton<IMachineInstanceInfo>(systemInfo);

        services.AddSingleton(systemInfo);

        services.AddSingleton<IExclusiveLock, ExclusiveLock>();

        services.AddTransient<IAggregatesDomainEventsRequestStore, AggregatesDomainEventsStore>();

        services.AddHttpContextAccessor();

        AddDefaultSerializer(services);

        services.AddMessagingCore(configuration, rootSectionName: rootSectionName);

        switch (configuration["IdGenerator:Type"])
        {
            case "Guid":
                services.AddSingleton<IIdGenerator<Guid>, GuidIdGenerator>();
                break;
            default:
                services.AddSingleton<IIdGenerator<long>, SnowFlakIdGenerator>();
                break;
        }

        return services;
    }


    private static void AddDefaultSerializer(
        IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        services.Add<ISerializer, DefaultSerializer>(lifetime);
        services.Add<IMessageSerializer, DefaultMessageSerializer>(lifetime);
    }
}
