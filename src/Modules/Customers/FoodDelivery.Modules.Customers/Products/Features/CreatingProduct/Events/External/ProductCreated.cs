using Squirrel.Abstractions.Messages;
using Squirrel.Abstractions.Persistence;
using Squirrel.Core.Messages;

namespace FoodDelivery.Modules.Customers.Products.Features.CreatingProduct.Events.External;

public record ProductCreated(long Id, string Name, long CategoryId, string CategoryName, int Stock) :
    IntegrationEvent, ITxRequest;

public class ProductCreatedConsumer : IMessageHandler<ProductCreated>
{
    public Task Handle(ProductCreated message, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
