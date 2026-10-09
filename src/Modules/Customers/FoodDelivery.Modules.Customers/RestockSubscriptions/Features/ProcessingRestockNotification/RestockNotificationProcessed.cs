using Ardalis.GuardClauses;
using Squirrel.Abstractions.Commands;
using Squirrel.Core.Events.Internal;
using FoodDelivery.Modules.Customers.RestockSubscriptions.Models.Write;

namespace FoodDelivery.Modules.Customers.RestockSubscriptions.Features.ProcessingRestockNotification;

public record RestockNotificationProcessed(RestockSubscription RestockSubscription) : DomainEvent;

internal class RestockNotificationProcessedHandler : IDomainEventHandler<RestockNotificationProcessed>
{
    private readonly ICommandBus _commandBus;

    public RestockNotificationProcessedHandler(ICommandBus commandBus)
    {
        _commandBus = commandBus;
    }

    public async Task Handle(RestockNotificationProcessed notification, CancellationToken cancellationToken)
    {
        Guard.Against.Null(notification, nameof(notification));

        // https://github.com/kgrzybek/modular-monolith-with-ddd#38-internal-processing
        await _commandBus.SendAsync(
            new UpdateMongoRestockSubscriptionReadModel(notification.RestockSubscription, false),
            cancellationToken);
    }
}
