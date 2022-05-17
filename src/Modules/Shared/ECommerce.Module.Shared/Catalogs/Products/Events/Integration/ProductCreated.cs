using BuildingBlocks.Core.Messaging;

namespace ECommerce.Modules.Shared.Catalogs.Products.Events.Integration;

public record ProductCreated(long Id, string Name, long CategoryId, string CategoryName, int Stock) :
    IntegrationEvent;
