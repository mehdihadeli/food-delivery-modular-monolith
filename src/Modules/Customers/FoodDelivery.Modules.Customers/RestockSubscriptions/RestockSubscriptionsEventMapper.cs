using Squirrel.Abstractions.Events;
using Squirrel.Core.Events.Internal;
using Squirrel.Abstractions.Messages;
using FoodDelivery.Modules.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.Events.Domain;

namespace FoodDelivery.Modules.Customers.RestockSubscriptions;

public class RestockSubscriptionsEventMapper : IIntegrationEventMapper
{
    public IReadOnlyList<IIntegrationEvent?> MapToIntegrationEvents(IReadOnlyList<IDomainEvent> domainEvents)
    {
        return domainEvents.Select(MapToIntegrationEvent).ToList();
    }

    public IIntegrationEvent? MapToIntegrationEvent(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            RestockSubscriptionCreated e =>
                new Features.CreatingRestockSubscription.Events.Integration.RestockSubscriptionCreated(
                    e.RestockSubscription.Id.Value, e.RestockSubscription.Email),
            _ => null
        };
    }
}
