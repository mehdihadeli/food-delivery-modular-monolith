using Asp.Versioning;
using Asp.Versioning.Builder;
using BuildingBlocks.Abstractions.Web.Module;
using BuildingBlocks.Core;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Messaging.Extensions;
using BuildingBlocks.Web.Extensions;
using ECommerce.Modules.Catalogs.Brands;
using ECommerce.Modules.Catalogs.Categories;
using ECommerce.Modules.Catalogs.Products;
using ECommerce.Modules.Catalogs.Shared.Extensions.ApplicationBuilderExtensions;
using ECommerce.Modules.Catalogs.Shared.Extensions.ServiceCollectionExtensions;
using ECommerce.Modules.Catalogs.Suppliers;

namespace ECommerce.Modules.Catalogs;

public class CatalogModuleConfiguration : IModuleDefinition
{
    public const string CatalogModulePrefixUri = "api/v{version:apiVersion}/catalogs";
    public const string ModuleName = "Catalogs";
    public void AddModuleServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddInfrastructure(configuration);
        services.AddStorage(configuration);

        // Add Sub Modules Services
        services.AddBrandsServices();
        services.AddCategoriesServices();
        services.AddSuppliersServices();
        services.AddProductsServices();
    }

    public async Task ConfigureModule(
        IApplicationBuilder app,
        IConfiguration configuration,
        ILogger logger,
        IWebHostEnvironment environment)
    {
        if (environment.IsEnvironment("test") == false)
        {
            // HostedServices just run on main service provider - It should not await because it will block the main thread.
            await app.ApplicationServices.StartHostedServices();
        }

        ServiceActivator.Configure(app.ApplicationServices);

        app.SubscribeAllMessageFromAssemblyOfType<CatalogRoot>();

        app.UseInfrastructure();

        await app.ApplyDatabaseMigrations(logger);
        await app.SeedData(logger, environment);
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Add Sub Modules Endpoints
        endpoints.MapProductsEndpoints();

        endpoints.MapGet("catalogs", (HttpContext context) =>
        {
            var requestId = context.Request.Headers.TryGetValue("X-Request-Id", out var requestIdHeader)
                ? requestIdHeader.FirstOrDefault()
                : string.Empty;

            return $"Catalogs Service Apis, RequestId: {requestId}";
        }).ExcludeFromDescription();
    }
}
