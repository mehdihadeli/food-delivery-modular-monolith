using BuildingBlocks.Core.Messaging;

namespace FoodDelivery.Modules.Catalogs.Products.Features.CreatingProduct.Events.Integration;

public record ProductCreated(long Id, string Name, long CategoryId, string CategoryName, int Stock) :
    IntegrationEvent;
