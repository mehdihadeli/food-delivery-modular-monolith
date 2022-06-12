using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Abstractions.Scheduling;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Core.Messaging.BackgroundServices;
using BuildingBlocks.Core.Messaging.Broker.InMemory;
using BuildingBlocks.Core.Messaging.MessagePersistence.InMemory;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Core.Registrations;

public static partial class InMemoryMessagingRegistrationExtensions
{
    public static IServiceCollection AddInMemoryMessagePersistence(
        this IServiceCollection services,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        services.Add<IMessagePersistenceRepository, InMemoryMessagePersistenceRepository>(serviceLifetime);

        services.Replace<IMessagePersistenceService, InMemoryMessagePersistenceService>(serviceLifetime);

        return services;
    }

    public static IServiceCollection AddInMemoryCommandScheduler(
        this IServiceCollection services,
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        services.Replace<ICommandScheduler, InMemoryCommandScheduler>(serviceLifetime);

        return services;
    }

    public static IServiceCollection AddInMemoryBroker(this IServiceCollection services, IConfiguration configuration)
    {
        services.ReplaceSingleton<IBus, InMemoryBus>();

        return services;
    }
}
