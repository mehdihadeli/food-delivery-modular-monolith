using Asp.Versioning.Builder;
using BuildingBlocks.Abstractions.CQRS.Event;
using BuildingBlocks.Abstractions.Persistence;
using ECommerce.Modules.Catalogs.Products.Data;
using ECommerce.Modules.Catalogs.Products.Features.CreatingProduct;
using ECommerce.Modules.Catalogs.Products.Features.DebitingProductStock;
using ECommerce.Modules.Catalogs.Products.Features.GettingProductById;
using ECommerce.Modules.Catalogs.Products.Features.GettingProductsView;
using ECommerce.Modules.Catalogs.Products.Features.ReplenishingProductStock;

namespace ECommerce.Modules.Catalogs.Products;

internal static class ProductsConfigs
{
    public const string Tag = "Product";
    public const string ProductsPrefixUri = $"{CatalogModuleConfiguration.CatalogModulePrefixUri}/products";
    public static ApiVersionSet VersionSet { get; private set; } = default!;

    internal static IServiceCollection AddProductsServices(this IServiceCollection services)
    {
        services.AddScoped<IDataSeeder, ProductDataSeeder>();
        services.AddSingleton<IEventMapper, ProductEventMapper>();

        return services;
    }

    internal static IEndpointRouteBuilder MapProductsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        VersionSet = endpoints.NewApiVersionSet(Tag).Build();

        return endpoints.MapCreateProductsEndpoint()
            .MapDebitProductStockEndpoint()
            .MapReplenishProductStockEndpoint()
            .MapGetProductByIdEndpoint()
            .MapGetProductsViewEndpoint();
    }
}
