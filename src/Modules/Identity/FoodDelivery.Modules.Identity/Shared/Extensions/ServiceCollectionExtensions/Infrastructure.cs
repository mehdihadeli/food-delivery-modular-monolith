using Mediator;
using Squirrel.Abstractions.Caching;
using Squirrel.Caching;
using Squirrel.Caching.Behaviors;
using Squirrel.Core.Extensions;
using Squirrel.Core.Pipelines;
using Squirrel.Core.Persistence.EfCore;
using Squirrel.Email;
using Squirrel.Security.Jwt;
using Squirrel.Validation;
using Squirrel.Validation.Extensions;

namespace FoodDelivery.Modules.Identity.Shared.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        var services = builder.Services;
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.Namespace = "FoodDelivery.Modules.Identity";
        });
        builder.AddCoreServices();
        services.AddControllers().AddControllersAsServices();
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>))
                .AddScoped(typeof(IStreamPipelineBehavior<,>), typeof(StreamRequestValidationBehavior<,>))
                .AddScoped(typeof(IStreamPipelineBehavior<,>), typeof(StreamLoggingBehavior<,>))
                .AddScoped(typeof(IStreamPipelineBehavior<,>), typeof(StreamCachingBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(InvalidateCachingBehavior<,>))
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(EfTxBehavior<,>));
            services.AddEmailService(configuration.GetSection(IdentityModuleConfiguration.ModuleName));

        services.AddCustomValidators(Assembly.GetExecutingAssembly());

        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        builder.AddCustomJwtAuthentication(configuration.GetSection(IdentityModuleConfiguration.ModuleName));
        services.AddCustomJwtAuthorization();
        services.AddOptions<CacheOptions>().Bind(configuration.GetSection(nameof(CacheOptions)));
        services.AddHybridCache();
        services.AddSingleton<ICacheService, CacheService>();

        services.AddSingleton<ILoggerFactory>(new Serilog.Extensions.Logging.SerilogLoggerFactory());

        return builder;
    }
}
