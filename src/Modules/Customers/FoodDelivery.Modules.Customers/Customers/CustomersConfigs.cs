using Asp.Versioning.Builder;
using Squirrel.Abstractions.Persistence;
using Squirrel.Abstractions.Web.Module;
using FoodDelivery.Modules.Customers.Customers.Data;

namespace FoodDelivery.Modules.Customers.Customers;

internal static class CustomersConfigs
{
    public const string Tag = "Customers";
    public const string CustomersPrefixUri = $"{CustomersModuleConfiguration.CustomerModulePrefixUri}";
    public static ApiVersionSet VersionSet { get; private set; } = default!;

    internal static IServiceCollection AddCustomersServices(this IServiceCollection services)
    {
        services.AddScoped<IDataSeeder<CustomersDbContext>, CustomersDataSeeder>();

        return services;
    }

    internal static IEndpointRouteBuilder MapCustomersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        VersionSet = endpoints.NewApiVersionSet(Tag).Build();

        // Routes mapped by conventions, with implementing `IMinimalEndpointDefinition` but we can map them here without convention.
        return endpoints;
    }
}
