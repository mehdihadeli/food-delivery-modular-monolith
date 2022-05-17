using BuildingBlocks.Core.Messaging;

namespace ECommerce.Modules.Shared.Customers.Customers.Events.Integration;

public record CustomerCreated(long CustomerId) : IntegrationEvent;
