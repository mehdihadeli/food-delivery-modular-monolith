using BuildingBlocks.Abstractions.CQRS.Command;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.Context;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Messaging;
using ECommerce.Modules.Customers.RestockSubscriptions.Features.ProcessingRestockNotification;

namespace ECommerce.Modules.Customers.Products.Features.ReplenishingProductStock.Events.External;

public record ProductStockReplenished(long ProductId, int NewStock, int ReplenishedQuantity) :
    IntegrationEvent,
    ITxRequest;

public class ProductStockReplenishedConsumer : IMessageHandler<ProductStockReplenished>
{
    private readonly ICommandProcessor _commandProcessor;
    private readonly ILogger<ProductStockReplenishedConsumer> _logger;

    public ProductStockReplenishedConsumer(
        ICommandProcessor commandProcessor,
        ILogger<ProductStockReplenishedConsumer> logger)
    {
        _commandProcessor = commandProcessor;
        _logger = logger;
    }

    // If this handler is called successfully, it will send a ACK to rabbitmq for removing message from the queue and if we have an exception it send an NACK to rabbitmq
    // and with NACK we can retry the message with re-queueing this message to the broker
    public async Task HandleAsync(
        IConsumeContext<ProductStockReplenished> messageContext,
        CancellationToken cancellationToken = default)
    {
        var productStockReplenished = messageContext.Message;

        await _commandProcessor.SendAsync(
            new ProcessRestockNotification(productStockReplenished.ProductId, productStockReplenished.NewStock),
            cancellationToken);

        _logger.LogInformation(
            "Sending restock notification command for product {ProductId}",
            productStockReplenished.ProductId);
    }
}
