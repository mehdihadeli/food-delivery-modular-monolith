using BuildingBlocks.Core.Messaging;

namespace FoodDelivery.Modules.Catalogs.Products.Features.DebitingProductStock.Events.Integration;

public record ProductStockDebited(long ProductId, int NewStock, int DebitedQuantity) : IntegrationEvent;
