using Ardalis.GuardClauses;
using Mediator;
using Squirrel.Abstractions.Caching;
using Squirrel.Caching;
using Squirrel.Caching.Behaviors;
using Squirrel.Core.Extensions;
using Squirrel.Core.Pipelines;
using Squirrel.Core.IdsGenerator;
using Squirrel.Core.Persistence.EfCore;
using Squirrel.Email;
using Squirrel.Email.Options;
using Squirrel.SerilogLogging;
using Squirrel.OpenTelemetry;
using Squirrel.Persistence.EfCore.Postgres;
using Squirrel.Validation;
using Squirrel.Validation.Extensions;

namespace FoodDelivery.Modules.Customers.Shared.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        var services = builder.Services;
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.Namespace = "FoodDelivery.Modules.Customers";
        });

        builder.AddCoreServices();
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>))
            .AddScoped(typeof(IStreamPipelineBehavior<,>), typeof(StreamRequestValidationBehavior<,>))
            .AddScoped(typeof(IStreamPipelineBehavior<,>), typeof(StreamLoggingBehavior<,>))
            .AddScoped(typeof(IStreamPipelineBehavior<,>), typeof(StreamCachingBehavior<,>))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(InvalidateCachingBehavior<,>))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(EfTxBehavior<,>));

        services.AddControllers().AddControllersAsServices();
        services.AddEmailService(configuration.GetSection(CustomersModuleConfiguration.ModuleName));

        services.AddCustomValidators(Assembly.GetExecutingAssembly());

        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddOptions<CacheOptions>().Bind(configuration.GetSection(nameof(CacheOptions)));
        services.AddHybridCache();
        services.AddSingleton<ICacheService, CacheService>();

        services.AddCustomHttpClients(configuration);

        services.AddSingleton<ILoggerFactory>(new Serilog.Extensions.Logging.SerilogLoggerFactory());

        return builder;
    }
}
