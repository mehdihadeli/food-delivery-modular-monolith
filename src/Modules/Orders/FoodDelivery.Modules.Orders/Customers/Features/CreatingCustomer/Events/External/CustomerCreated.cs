using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.Context;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Messaging;

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
