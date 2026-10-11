using Mediator;
using Squirrel.Abstractions.Commands;
using Squirrel.Abstractions.Events;
using Squirrel.Abstractions.Messages;
using Squirrel.Abstractions.Messages.MessagePersistence;

namespace FoodDelivery.Api.Messaging;

public sealed class ModuleMessagePersistenceService(IExternalEventBus eventBus, IMediator mediator)
    : IMessagePersistenceService
{
    public Task AddPublishMessageAsync(IMessageEnvelopeBase messageEnvelope, CancellationToken cancellationToken = default) =>
        eventBus.PublishAsync(messageEnvelope, cancellationToken);

    public Task AddReceivedMessageAsync<TMessage>(IMessageEnvelopeBase messageEnvelope,
        Func<IMessageEnvelopeBase, Task> dispatchAction, CancellationToken cancellationToken = default) =>
        dispatchAction(messageEnvelope);

    public async Task AddInternalMessageAsync<TInternalCommand>(TInternalCommand internalCommand,
        CancellationToken cancellationToken = default) where TInternalCommand : IInternalCommand =>
        await mediator.Send(internalCommand, cancellationToken);

    public async Task AddNotificationAsync<TDomainNotification>(TDomainNotification notification,
        CancellationToken cancellationToken = default) where TDomainNotification : IDomainNotificationEvent<IDomainEvent> =>
        await mediator.Publish(notification, cancellationToken);
}
