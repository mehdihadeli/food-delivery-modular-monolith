using Squirrel.Abstractions.Persistence;
using FoodDelivery.Modules.Catalogs.Shared.Data;
using FoodDelivery.Modules.Catalogs.Categories.Data;

namespace FoodDelivery.Modules.Catalogs.Categories;

internal static class Configs
{
    internal static IServiceCollection AddCategoriesServices(this IServiceCollection services)
    {
        services.AddScoped<IDataSeeder<CatalogDbContext>, CategoryDataSeeder>();

        return services;
    }

    internal static IEndpointRouteBuilder MapCategoriesEndpoints(this IEndpointRouteBuilder endpoints)
    {

        return endpoints;
    }
}
