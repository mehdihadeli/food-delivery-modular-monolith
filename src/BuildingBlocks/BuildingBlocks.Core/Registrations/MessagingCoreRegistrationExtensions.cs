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
        string? rootSectionName = null,
        params Assembly[] assembliesToScan)
    {
        AddHandlers(services, serviceLifetime, assembliesToScan);
        AddPersistenceMessage(services, configuration, serviceLifetime, rootSectionName);
        services.AddSingleton<IBus, NullBus>();
        services.AddHostedService<BusBackgroundService>();
    }

    internal static void AddPersistenceMessage(
        IServiceCollection services,
        IConfiguration configuration,
        ServiceLifetime serviceLifetime,
        string? rootSectionName = null)
    {
        services.Add<IMessagePersistenceService, NullMessagePersistenceService>(serviceLifetime);
        services.AddHostedService<MessagePersistenceBackgroundService>();

        var section = string.IsNullOrEmpty(rootSectionName)
            ? nameof(MessagePersistenceOptions)
            : $"{rootSectionName}:{nameof(MessagePersistenceOptions)}";

        services.AddOptions<MessagePersistenceOptions>()
            .Bind(configuration.GetSection(section))
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
