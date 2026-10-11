using Squirrel.Abstractions.Persistence;
using FoodDelivery.Modules.Catalogs.Shared.Data;
using FoodDelivery.Modules.Catalogs.Brands.Data;

namespace FoodDelivery.Modules.Catalogs.Brands;

internal static class Configs
{
    internal static IServiceCollection AddBrandsServices(this IServiceCollection services)
    {
        services.AddScoped<IDataSeeder<CatalogDbContext>, BrandDataSeeder>();

        return services;
    }

    internal static IEndpointRouteBuilder MapBrandsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        return endpoints;
    }
}
