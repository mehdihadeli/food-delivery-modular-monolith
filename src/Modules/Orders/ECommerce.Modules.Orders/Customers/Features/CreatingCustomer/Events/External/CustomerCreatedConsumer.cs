using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.Context;
using ECommerce.Modules.Shared.Customers.Customers.Events.Integration;

namespace ECommerce.Modules.Orders.Customers.Features.CreatingCustomer.Events.External;

public class CustomerCreatedConsumer : IMessageHandler<CustomerCreated>
{
    public Task HandleAsync(IConsumeContext<CustomerCreated> messageContext, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
