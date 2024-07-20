using BuildingBlocks.Resiliency.Extensions;
using FoodDelivery.Modules.Customers.Shared.Clients.Catalogs;
using FoodDelivery.Modules.Customers.Shared.Clients.Identity;

namespace FoodDelivery.Modules.Customers.Shared.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomHttpClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<IdentityApiClientOptions>().Bind(configuration.GetSection(
                $"{CustomersModuleConfiguration.ModuleName}:{nameof(IdentityApiClientOptions)}"))
            .ValidateDataAnnotations();

        services.AddOptions<CatalogsApiClientOptions>().Bind(
                configuration.GetSection(
                    $"{CustomersModuleConfiguration.ModuleName}:{nameof(CatalogsApiClientOptions)}"))
            .ValidateDataAnnotations();

        services.AddHttpApiClient<ICatalogApiClient, CatalogApiClient>();
        services.AddHttpApiClient<IIdentityApiClient, IdentityApiClient>();

        return services;
    }
}
