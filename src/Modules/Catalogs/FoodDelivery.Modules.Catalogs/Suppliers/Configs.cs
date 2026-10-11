using Squirrel.Abstractions.Persistence;
using FoodDelivery.Modules.Catalogs.Shared.Data;
using FoodDelivery.Modules.Catalogs.Suppliers.Data;

namespace FoodDelivery.Modules.Catalogs.Suppliers;

internal static class Configs
{
    internal static IServiceCollection AddSuppliersServices(this IServiceCollection services)
    {
        services.AddScoped<IDataSeeder<CatalogDbContext>, SupplierDataSeeder>();

        return services;
    }

    internal static IEndpointRouteBuilder MapSuppliersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        return endpoints;
    }
}
