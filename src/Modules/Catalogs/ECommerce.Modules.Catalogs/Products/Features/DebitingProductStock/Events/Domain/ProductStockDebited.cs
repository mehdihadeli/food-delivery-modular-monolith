using BuildingBlocks.Core.CQRS.Event.Internal;
using ECommerce.Modules.Catalogs.Products.ValueObjects;

namespace ECommerce.Modules.Catalogs.Products.Features.DebitingProductStock.Events.Domain;

public record ProductStockDebited(ProductId ProductId, Stock NewStock, int DebitedQuantity) : DomainEvent;
