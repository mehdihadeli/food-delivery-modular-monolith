using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.Context;
using ECommerce.Modules.Shared.Catalogs.Products.Events.Integration;

namespace ECommerce.Modules.Customers.Products.Features.CreatingProduct.Events.External;

public class ProductCreatedConsumer : IMessageHandler<ProductCreated>
{
    public Task HandleAsync(IConsumeContext<ProductCreated> messageContext, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
