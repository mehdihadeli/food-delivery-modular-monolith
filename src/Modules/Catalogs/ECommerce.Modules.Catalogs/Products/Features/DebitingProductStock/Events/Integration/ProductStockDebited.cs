using BuildingBlocks.Core.Messaging;

namespace ECommerce.Modules.Catalogs.Products.Features.DebitingProductStock.Events.Integration;

public record ProductStockDebited(long ProductId, int NewStock, int DebitedQuantity) : IntegrationEvent;
