using Squirrel.Abstractions.Commands;
using Squirrel.Abstractions.Messages;
using Squirrel.Abstractions.Persistence;
using Squirrel.Core.Messages;
using FoodDelivery.Modules.Customers.RestockSubscriptions.Features.ProcessingRestockNotification;

namespace FoodDelivery.Modules.Customers.Products.Features.ReplenishingProductStock.Events.External;

public record ProductStockReplenished(long ProductId, int NewStock, int ReplenishedQuantity) :
    IntegrationEvent,
    ITxRequest;

public class ProductStockReplenishedConsumer : IMessageHandler<ProductStockReplenished>
{
    private readonly ICommandBus _commandBus;
    private readonly ILogger<ProductStockReplenishedConsumer> _logger;

    public ProductStockReplenishedConsumer(
        ICommandBus commandBus,
        ILogger<ProductStockReplenishedConsumer> logger)
    {
        _commandBus = commandBus;
        _logger = logger;
    }

    // If this handler is called successfully, it will send a ACK to rabbitmq for removing message from the queue and if we have an exception it send an NACK to rabbitmq
    // and with NACK we can retry the message with re-queueing this message to the broker
    public async Task Handle(
        ProductStockReplenished productStockReplenished,
        CancellationToken cancellationToken = default)
    {
        await _commandBus.SendAsync(
            new ProcessRestockNotification(productStockReplenished.ProductId, productStockReplenished.NewStock),
            cancellationToken);

        _logger.LogInformation(
            "Sending restock notification command for product {ProductId}",
            productStockReplenished.ProductId);
    }
}
