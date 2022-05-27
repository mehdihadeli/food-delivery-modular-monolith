using BuildingBlocks.Core.Messaging;

namespace ECommerce.Modules.Customers.Customers.Features.CreatingCustomer.Events.Integration;

public record CustomerCreated(long CustomerId) : IntegrationEvent;
