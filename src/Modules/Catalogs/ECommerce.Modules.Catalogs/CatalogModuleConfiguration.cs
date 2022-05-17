using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.Web.Module;
using BuildingBlocks.Caching.InMemory;
using BuildingBlocks.Core;
using BuildingBlocks.Core.Caching;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.IdsGenerator;
using BuildingBlocks.Core.Persistence.EfCore;
using BuildingBlocks.Core.Registrations;
using BuildingBlocks.Email;
using BuildingBlocks.Integration.MassTransit;
using BuildingBlocks.Logging;
using BuildingBlocks.Monitoring;
using BuildingBlocks.Persistence.EfCore.Postgres;
using BuildingBlocks.Validation;
using ECommerce.Modules.Catalogs.Brands;
using ECommerce.Modules.Catalogs.Categories;
using ECommerce.Modules.Catalogs.Products;
using ECommerce.Modules.Catalogs.Shared.Extensions.ApplicationBuilderExtensions;
using ECommerce.Modules.Catalogs.Shared.Extensions.ServiceCollectionExtensions;
using ECommerce.Modules.Catalogs.Suppliers;

namespace ECommerce.Modules.Catalogs;

public class CatalogModuleConfiguration : IRootModuleDefinition
{
    public const string CatalogModulePrefixUri = "api/v1/catalogs";

    public IServiceCollection AddModuleServices(IServiceCollection services, IConfiguration configuration)
    {
        SnowFlakIdGenerator.Configure(1);

        services.AddCore(configuration);

        services.AddEmailService(configuration);

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

        services.AddInMemoryMessagePersistence();
        services.AddCustomMassTransit(
            configuration,
            (busRegistrationContext, busFactoryConfigurator) =>
            {
                busFactoryConfigurator.AddProductPublishers();
            });

        services.AddMonitoring(healthChecksBuilder =>
        {
            var postgresOptions = configuration.GetOptions<PostgresOptions>(nameof(PostgresOptions));
            Guard.Against.Null(postgresOptions, nameof(postgresOptions));

            healthChecksBuilder.AddNpgSql(
                postgresOptions.ConnectionString,
                name: "CatalogsService-Postgres-Check",
                tags: new[] {"catalogs-postgres"});
        });

        services.AddCustomValidators(Assembly.GetExecutingAssembly());
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddCustomInMemoryCache(configuration)
            .AddCachingRequestPolicies(Assembly.GetExecutingAssembly());


        services.AddStorage(configuration);

        services.AddBrandsServices();
        services.AddCategoriesServices();
        services.AddSuppliersServices();

        services.AddProductsServices();

        return services;
    }

    public async Task<WebApplication> ConfigureModule(WebApplication app)
    {
        ServiceActivator.Configure(app.Services);

        app.UseMonitoring();

        await app.ApplyDatabaseMigrations(app.Logger);
        await app.SeedData(app.Logger, app.Environment);

        return app;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", (HttpContext context) =>
        {
            var requestId = context.Request.Headers.TryGetValue("X-Request-Id", out var requestIdHeader)
                ? requestIdHeader.FirstOrDefault()
                : string.Empty;

            return $"Catalogs Service Apis, RequestId: {requestId}";
        }).ExcludeFromDescription();

        endpoints.MapProductsEndpoints();

        return endpoints;
    }
}
