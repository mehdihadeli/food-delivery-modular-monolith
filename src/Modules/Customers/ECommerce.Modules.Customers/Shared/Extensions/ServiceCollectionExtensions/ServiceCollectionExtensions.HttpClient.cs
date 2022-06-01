using BuildingBlocks.Resiliency.Extensions;
using ECommerce.Modules.Customers.Shared.Clients.Catalogs;
using ECommerce.Modules.Customers.Shared.Clients.Identity;

namespace ECommerce.Modules.Customers.Shared.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomHttpClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var s = configuration.AsEnumerable().ToList();
        var f = configuration.GetSection(CustomersModuleConfiguration.ModuleName).Value;
        var f1 = configuration.GetSection($"{CustomersModuleConfiguration.ModuleName}:{nameof(IdentityApiClientOptions)}");

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
