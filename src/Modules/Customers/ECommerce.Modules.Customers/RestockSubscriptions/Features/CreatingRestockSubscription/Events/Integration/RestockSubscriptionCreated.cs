using BuildingBlocks.Core.Messaging;

namespace ECommerce.Modules.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.Events.Integration;

public record RestockSubscriptionCreated(long CustomerId, string? Email) : IntegrationEvent;

