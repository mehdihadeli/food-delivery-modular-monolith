using Squirrel.Core.Events.Internal;
using FoodDelivery.Modules.Catalogs.Products.ValueObjects;

namespace FoodDelivery.Modules.Catalogs.Products.Features.DebitingProductStock.Events.Domain;

public record ProductStockDebited(ProductId ProductId, Stock NewStock, int DebitedQuantity) : DomainEvent;
