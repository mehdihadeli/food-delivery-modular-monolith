using ECommerce.Modules.Customers.Shared.Clients.Catalogs.Dtos;

namespace ECommerce.Modules.Customers.Shared.Clients.Catalogs;

public interface ICatalogApiClient
{
    Task<GetProductByIdResponse?> GetProductByIdAsync(long id, CancellationToken cancellationToken = default);
}
