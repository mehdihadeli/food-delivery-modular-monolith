using BuildingBlocks.Abstractions.Persistence;
using FoodDelivery.Modules.Catalogs.Brands.Data;

namespace FoodDelivery.Modules.Catalogs.Brands;

internal static class Configs
{
    internal static IServiceCollection AddBrandsServices(this IServiceCollection services)
    {
        services.AddScoped<IDataSeeder, BrandDataSeeder>();

        return services;
    }

    internal static IEndpointRouteBuilder MapBrandsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        return endpoints;
    }
}
