using BuildingBlocks.Abstractions.CQRS.Query;

namespace ECommerce.Modules.Catalogs.Products.Features.GettingAvailableStockById;

public record GetAvailableStockById(long ProductId) : IQuery<int>;

