using BuildingBlocks.Core.Messaging;

namespace ECommerce.Modules.Shared.Customers.RestockSubscriptions.Events.Integration;

public record RestockSubscriptionCreated(long CustomerId, string? Email) : IntegrationEvent;
