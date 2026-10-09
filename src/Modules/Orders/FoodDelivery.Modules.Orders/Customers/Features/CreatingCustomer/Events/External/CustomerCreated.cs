using Squirrel.Abstractions.Messages;
using Squirrel.Abstractions.Messages.Context;
using Squirrel.Abstractions.Persistence;
using Squirrel.Core.Messages;

namespace FoodDelivery.Modules.Orders.Customers.Features.CreatingCustomer.Events.External;

public record CustomerCreated(long CustomerId) : IntegrationEvent, ITxRequest;

public class CustomerCreatedConsumer : IMessageHandler<CustomerCreated>
{
    public Task HandleAsync(
        IConsumeContext<CustomerCreated> messageContext,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
