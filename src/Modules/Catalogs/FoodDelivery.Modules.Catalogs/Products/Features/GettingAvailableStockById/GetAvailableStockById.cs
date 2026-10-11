using Squirrel.Abstractions.Queries;

namespace FoodDelivery.Modules.Catalogs.Products.Features.GettingAvailableStockById;

public record GetAvailableStockById(long ProductId) : IQuery<int>;

