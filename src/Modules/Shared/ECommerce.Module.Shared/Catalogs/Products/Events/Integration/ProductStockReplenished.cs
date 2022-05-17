using BuildingBlocks.Core.Messaging;

namespace ECommerce.Modules.Shared.Catalogs.Products.Events.Integration;

public record ProductStockReplenished(long ProductId, int NewStock, int ReplenishedQuantity) : IntegrationEvent;
