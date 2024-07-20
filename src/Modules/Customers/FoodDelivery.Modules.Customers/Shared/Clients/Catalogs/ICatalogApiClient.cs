using FoodDelivery.Modules.Customers.Shared.Clients.Catalogs.Dtos;

namespace FoodDelivery.Modules.Customers.Shared.Clients.Catalogs;

// Ref: http://www.kamilgrzybek.com/design/modular-monolith-integration-styles/
// https://docs.microsoft.com/en-us/azure/architecture/patterns/anti-corruption-layer
public interface ICatalogApiClient
{
    Task<GetProductByIdResponse?> GetProductByIdAsync(long id, CancellationToken cancellationToken = default);
}
