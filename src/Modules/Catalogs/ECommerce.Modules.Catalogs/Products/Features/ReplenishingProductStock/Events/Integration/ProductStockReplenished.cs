using BuildingBlocks.Core.Messaging;

namespace ECommerce.Modules.Catalogs.Products.Features.ReplenishingProductStock.Events.Integration;

public record ProductStockReplenished(long ProductId, int NewStock, int ReplenishedQuantity) : IntegrationEvent;
