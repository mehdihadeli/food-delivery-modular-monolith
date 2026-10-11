using Squirrel.Abstractions.Events;
using Squirrel.Core.Events.Internal;
using Squirrel.Abstractions.Messages;
using FoodDelivery.Modules.Customers.Customers.Features.CreatingCustomer.Events.Domain;

namespace FoodDelivery.Modules.Customers.Customers;

public class CustomersEventMapper : IIntegrationEventMapper
{
    public IReadOnlyList<IIntegrationEvent?> MapToIntegrationEvents(IReadOnlyList<IDomainEvent> domainEvents)
    {
        return domainEvents.Select(MapToIntegrationEvent).ToList();
    }

    public IIntegrationEvent? MapToIntegrationEvent(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            CustomerCreated e => new Features.CreatingCustomer.Events.Integration.CustomerCreated(e.Customer.Id),
            _ => null
        };
    }
}
