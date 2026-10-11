using Asp.Versioning;
using Asp.Versioning.Builder;
using Squirrel.Abstractions.Web.Module;
using Squirrel.Web.Module;
using Squirrel.Core.Extensions;
using Squirrel.Core.Messages.Extensions;
using Squirrel.Web.Extensions;
using FoodDelivery.Modules.Catalogs.Brands;
using FoodDelivery.Modules.Catalogs.Categories;
using FoodDelivery.Modules.Catalogs.Products;
using FoodDelivery.Modules.Catalogs.Shared.Extensions.ApplicationBuilderExtensions;
using FoodDelivery.Modules.Catalogs.Shared.Extensions.ServiceCollectionExtensions;
using FoodDelivery.Modules.Catalogs.Suppliers;

namespace FoodDelivery.Modules.Catalogs;

public class CatalogModuleConfiguration : IModuleConfiguration
{
    public const string CatalogModulePrefixUri = "api/v{version:apiVersion}/catalogs";
    public const string ModuleName = "Catalogs";
    public WebApplicationBuilder AddModuleServices(WebApplicationBuilder builder)
    {
        var services = builder.Services;
        builder.AddInfrastructure(builder.Configuration);
        builder.AddStorage(builder.Configuration);

        // Add Sub Modules Services
        services.AddBrandsServices();
        services.AddCategoriesServices();
        services.AddSuppliersServices();
        services.AddProductsServices();
        return builder;
    }

    public async Task<WebApplication> ConfigureModule(WebApplication app)
    {
        var moduleProvider = CompositionRootRegistry.GetByModule<CatalogModuleConfiguration>()?.ServiceProvider;
        if (app.Environment.IsEnvironment("test") == false
            && moduleProvider is not null
            && !ReferenceEquals(moduleProvider, app.Services))
        {
            foreach (var hostedService in moduleProvider.GetServices<IHostedService>())
            {
                await hostedService.StartAsync(app.Lifetime.ApplicationStopping);
            }
        }

        app.UseInfrastructure();
        var moduleApp = ((IApplicationBuilder)app).New();
        moduleApp.ApplicationServices = moduleProvider ?? app.Services;
        await moduleApp.ApplyDatabaseMigrations(app.Logger);
        await moduleApp.SeedData(app.Logger, app.Environment);
        return app;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
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
        return endpoints;
    }
}
