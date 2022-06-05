using System.Collections.Immutable;
using System.Linq.Expressions;
using BuildingBlocks.Abstractions.CQRS.Command;
using BuildingBlocks.Abstractions.CQRS.Event.Internal;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;

namespace BuildingBlocks.Core.Messaging.MessagePersistence;

public class NullMessagePersistenceService : IMessagePersistenceService
{
    public event Action<StoreMessage, MessageDeliveryType>? MessageProcessed;

    public Task<IReadOnlyList<StoreMessage>> GetByFilterAsync(
        Expression<Func<StoreMessage, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<StoreMessage>>(new List<StoreMessage>());
    }

    public Task AddPublishMessageAsync<TMessageEnvelope>(
        TMessageEnvelope messageEnvelope,
        CancellationToken cancellationToken = default)
        where TMessageEnvelope : MessageEnvelope
    {
        return Task.CompletedTask;
    }

    public Task AddReceivedMessageAsync<TMessageEnvelope>(
        TMessageEnvelope messageEnvelope,
        CancellationToken cancellationToken = default)
        where TMessageEnvelope : MessageEnvelope
    {
        return Task.CompletedTask;
    }

    public Task AddInternalMessageAsync<TCommand>(TCommand internalCommand, CancellationToken cancellationToken = default)
        where TCommand : class, IInternalCommand
    {
       return Task.CompletedTask;
    }

    public Task AddNotificationAsync(
        IDomainNotificationEvent notification,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task ProcessAsync(
        Guid messageId,
        MessageDeliveryType deliveryType,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task ProcessAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
