using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.Context;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Messaging;

namespace ECommerce.Modules.Customers.Products.Features.CreatingProduct.Events.External;

public record ProductCreated(long Id, string Name, long CategoryId, string CategoryName, int Stock) :
    IntegrationEvent, ITxRequest;

public class ProductCreatedConsumer : IMessageHandler<ProductCreated>
{
    public Task HandleAsync(
        IConsumeContext<ProductCreated> messageContext,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
