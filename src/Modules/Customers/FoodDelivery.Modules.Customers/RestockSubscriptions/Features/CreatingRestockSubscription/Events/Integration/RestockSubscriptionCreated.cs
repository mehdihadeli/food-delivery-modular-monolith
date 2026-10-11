using Squirrel.Core.Messages;

namespace FoodDelivery.Modules.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.Events.Integration;

public record RestockSubscriptionCreated(long CustomerId, string? Email) : IntegrationEvent;

