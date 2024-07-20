using BuildingBlocks.Abstractions.CQRS.Query;

namespace FoodDelivery.Modules.Catalogs.Products.Features.GettingAvailableStockById;

public record GetAvailableStockById(long ProductId) : IQuery<int>;

