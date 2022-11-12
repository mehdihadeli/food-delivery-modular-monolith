using Ardalis.GuardClauses;
using BuildingBlocks.Caching.InMemory;
using BuildingBlocks.Core.Caching;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.IdsGenerator;
using BuildingBlocks.Core.Persistence.EfCore;
using BuildingBlocks.Core.Registrations;
using BuildingBlocks.Email;
using BuildingBlocks.Email.Options;
using BuildingBlocks.Logging;
using BuildingBlocks.Validation;
using BuildingBlocks.Web.Extensions.ServiceCollectionExtensions;

namespace ECommerce.Modules.Catalogs.Shared.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCqrs(doMoreActions: s =>
        {
            s.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>))
                .AddScoped(typeof(IStreamPipelineBehavior<,>), typeof(StreamRequestValidationBehavior<,>))
                .AddScoped(typeof(IStreamPipelineBehavior<,>), typeof(StreamLoggingBehavior<,>))
                .AddScoped(typeof(IStreamPipelineBehavior<,>), typeof(StreamCachingBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(InvalidateCachingBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(EfTxBehavior<,>));
        });

        services.AddControllersAsServices();

        SnowFlakIdGenerator.Configure(1);

        services.AddCore(
            configuration,
            CatalogModuleConfiguration.ModuleName,
            Assembly.GetExecutingAssembly());

        services.AddEmailService(configuration, $"{CatalogModuleConfiguration.ModuleName}:{nameof(EmailOptions)}");

        services.AddInMemoryMessagePersistence();
        services.AddInMemoryCommandScheduler();
        services.AddInMemoryBroker(configuration);

        services.AddCustomValidators(Assembly.GetExecutingAssembly());
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddCustomInMemoryCache(configuration)
            .AddCachingRequestPolicies(Assembly.GetExecutingAssembly());


        services.AddSingleton<ILoggerFactory>(new Serilog.Extensions.Logging.SerilogLoggerFactory());

        return services;
    }
}
