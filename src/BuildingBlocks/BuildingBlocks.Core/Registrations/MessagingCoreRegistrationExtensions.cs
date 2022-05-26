using System.Reflection;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Core.Messaging.BackgroundServices;
using BuildingBlocks.Core.Messaging.Broker;
using BuildingBlocks.Core.Messaging.MessagePersistence;
using Microsoft.Extensions.Configuration;
using Scrutor;

namespace BuildingBlocks.Core.Registrations;

public static class MessagingCoreRegistrationExtensions
{
    internal static void AddMessagingCore(
        this IServiceCollection services,
        IConfiguration configuration,
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient,
        params Assembly[] assembliesToScan)
    {
        AddHandlers(services, serviceLifetime, assembliesToScan);
        AddPersistenceMessage(services, configuration, serviceLifetime);
        services.AddSingleton<IBus, NullBus>();
        services.AddHostedService<BusBackgroundService>();
    }

    internal static void AddPersistenceMessage(
        IServiceCollection services,
        IConfiguration configuration,
        ServiceLifetime serviceLifetime)
    {
        services.Add<IMessagePersistenceService, NullMessagePersistenceService>(serviceLifetime);
        services.AddHostedService<MessagePersistenceBackgroundService>();
        services.AddOptions<MessagePersistenceOptions>()
            .Bind(configuration.GetSection(nameof(MessagePersistenceOptions)))
            .ValidateDataAnnotations();
    }

    private static void AddHandlers(
        IServiceCollection services,
        ServiceLifetime serviceLifetime,
        Assembly[] assembliesToScan)
    {
        services.Scan(scan => scan
            .FromAssemblies(assembliesToScan.Any() ? assembliesToScan : AppDomain.CurrentDomain.GetAssemblies())
            .AddClasses(classes => classes.AssignableTo(typeof(IMessageHandler<>)))
            .UsingRegistrationStrategy(RegistrationStrategy.Append)
            .AsClosedTypeOf(typeof(IMessageHandler<>))
            .AsSelf()
            .WithLifetime(serviceLifetime));
    }
}
