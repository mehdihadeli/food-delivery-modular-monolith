using BuildingBlocks.Core.Messaging;

namespace FoodDelivery.Modules.Customers.Customers.Features.CreatingCustomer.Events.Integration;

public record CustomerCreated(long CustomerId) : IntegrationEvent;
