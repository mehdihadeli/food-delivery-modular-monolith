using BuildingBlocks.Abstractions.Web.Module;
using BuildingBlocks.Core;
using BuildingBlocks.Core.Messaging.Extensions;
using ECommerce.Modules.Catalogs.Brands;
using ECommerce.Modules.Catalogs.Categories;
using ECommerce.Modules.Catalogs.Products;
using ECommerce.Modules.Catalogs.Shared.Extensions.ApplicationBuilderExtensions;
using ECommerce.Modules.Catalogs.Shared.Extensions.ServiceCollectionExtensions;
using ECommerce.Modules.Catalogs.Suppliers;

namespace ECommerce.Modules.Catalogs;

public class CatalogModuleConfiguration : IModuleDefinition
{
    public const string CatalogModulePrefixUri = "api/v1/catalogs";
    public const string ModuleName = "Catalogs";

    public void AddModuleServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        services.AddStorage(configuration);

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
        ServiceActivator.Configure(app.ApplicationServices);

        app.SubscribeAllMessageFromAssemblyOfType<CatalogRoot>();

        await app.ApplyDatabaseMigrations(logger);
        await app.SeedData(logger, environment);
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
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
