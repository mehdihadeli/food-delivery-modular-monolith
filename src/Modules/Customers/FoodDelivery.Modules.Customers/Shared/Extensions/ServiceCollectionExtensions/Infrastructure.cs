using Ardalis.GuardClauses;
using Squirrel.Caching;
using Squirrel.Core.Extensions;
using Squirrel.Core.IdsGenerator;
using Squirrel.Core.Persistence.EfCore;
using Squirrel.Email;
using Squirrel.Email.Options;
using Squirrel.SerilogLogging;
using Squirrel.OpenTelemetry;
using Squirrel.Persistence.EfCore.Postgres;
using Squirrel.Validation;
using Squirrel.Web.Extensions.ServiceCollectionExtensions;

namespace FoodDelivery.Modules.Customers.Shared.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCqrs(
            doMoreActions: s =>
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

        SnowFlakIdGenerator.Configure(2);

        services.AddCore(
            configuration,
            CustomersModuleConfiguration.ModuleName,
            Assembly.GetExecutingAssembly());

        services.AddEmailService(configuration, $"{CustomersModuleConfiguration.ModuleName}:{nameof(EmailOptions)}");

        services.AddInMemoryMessagePersistence();
        services.AddInMemoryCommandScheduler();
        services.AddInMemoryBroker(configuration);

        services.AddCustomValidators(Assembly.GetExecutingAssembly());

        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddCustomInMemoryCache(configuration)
            .AddCachingRequestPolicies(Assembly.GetExecutingAssembly());

        services.AddCustomHttpClients(configuration);

        services.AddSingleton<ILoggerFactory>(new Serilog.Extensions.Logging.SerilogLoggerFactory());

        return services;
    }
}
