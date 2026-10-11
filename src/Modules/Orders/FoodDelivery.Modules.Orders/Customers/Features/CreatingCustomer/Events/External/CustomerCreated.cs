using Squirrel.Abstractions.Messages;
using Squirrel.Abstractions.Persistence;
using Squirrel.Core.Messages;

namespace FoodDelivery.Modules.Orders.Customers.Features.CreatingCustomer.Events.External;

public record CustomerCreated(long CustomerId) : IntegrationEvent, ITxRequest;

public class CustomerCreatedConsumer : IMessageHandler<CustomerCreated>
{
    public Task Handle(
        CustomerCreated message,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
