using System.Reflection;
using BuildingBlocks.Abstractions.CQRS.Command;
using BuildingBlocks.Abstractions.CQRS.Event;
using BuildingBlocks.Abstractions.CQRS.Event.Internal;
using BuildingBlocks.Abstractions.CQRS.Query;
using BuildingBlocks.Abstractions.Scheduling;
using BuildingBlocks.Core.CQRS.Command;
using BuildingBlocks.Core.CQRS.Event;
using BuildingBlocks.Core.CQRS.Query;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Core.Scheduler;
using MediatR;

namespace BuildingBlocks.Core.Registrations;

public static class CQRSRegistrationExtensions
{
    public static IServiceCollection AddCqrs(
        this IServiceCollection services,
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient,
        Action<IServiceCollection>? doMoreActions = null,
        params Assembly[] assemblies)
    {
        var assembliesToScan = assemblies.Any() ? assemblies : new[] {Assembly.GetCallingAssembly()};

        services.AddMediatR(
            assembliesToScan,
            x =>
            {
                switch (serviceLifetime)
                {
                    case ServiceLifetime.Transient:
                        x.AsTransient();
                        break;
                    case ServiceLifetime.Scoped:
                        x.AsScoped();
                        break;
                    case ServiceLifetime.Singleton:
                        x.AsSingleton();
                        break;
                }
            });

        services.Add<ICommandProcessor, CommandProcessor>(serviceLifetime)
            .Add<IQueryProcessor, QueryProcessor>(serviceLifetime)
            .Add<IEventProcessor, EventProcessor>(serviceLifetime)
            .Add<IScheduler, NullScheduler>(serviceLifetime)
            .Add<ICommandScheduler, NullCommandScheduler>(serviceLifetime)
            .Add<IDomainEventPublisher, DomainEventPublisher>(serviceLifetime)
            .Add<IDomainNotificationEventPublisher, DomainNotificationEventPublisher>(serviceLifetime);

        RegisterEventMappers(services, assembliesToScan);

        services.AddScoped<IDomainEventsAccessor, NullDomainEventsAccessor>();

        doMoreActions?.Invoke(services);

        return services;
    }

    private static void RegisterEventMappers(IServiceCollection services, params Assembly[] assembliesToScan)
    {
        services.Scan(scan => scan
            .FromAssemblies(assembliesToScan.Any() ? assembliesToScan : AppDomain.CurrentDomain.GetAssemblies())
            .AddClasses(classes => classes.AssignableTo(typeof(IEventMapper)), false)
            .AsImplementedInterfaces()
            .WithSingletonLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IIntegrationEventMapper)), false)
            .AsImplementedInterfaces()
            .WithSingletonLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IIDomainNotificationEventMapper)), false)
            .AsImplementedInterfaces()
            .WithSingletonLifetime());
    }
}
