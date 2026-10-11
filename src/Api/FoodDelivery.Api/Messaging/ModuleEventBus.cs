using Squirrel.Abstractions.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Wolverine.RDBMS;
using IMessage = Squirrel.Abstractions.Messages.IMessage;

namespace FoodDelivery.Api.Messaging;

public sealed class ModuleEventBus(ModuleMessagingDatabase database, ModuleMessagingSession session) : IExternalEventBus
{
    private async Task StoreAsync(IMessage message, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var transaction = database.Context.Database.CurrentTransaction
            ?? throw new InvalidOperationException("Integration events require the module's business transaction.");
        var dbTransaction = transaction.GetDbTransaction();
        session.Track(dbTransaction);
        var messaging = session.Context;
        if (messaging.PreviewSubscriptions(message).Count == 0)
        {
            throw new InvalidOperationException($"No integration event route registered for {message.GetType().FullName}.");
        }
        if (messaging.Transaction is null)
        {
            await messaging.EnlistInOutboxAsync(new DatabaseEnvelopeTransaction(
                (IMessageDatabase)messaging.Storage, dbTransaction));
        }
        await messaging.PublishAsync(message);
    }

    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class, IMessage => StoreAsync(message, cancellationToken);

    public Task PublishAsync<TMessage>(IMessageEnvelope<TMessage> messageEnvelope, CancellationToken cancellationToken = default)
        where TMessage : class, IMessage => StoreAsync(messageEnvelope.Message, cancellationToken);

    public Task PublishAsync(IMessageEnvelopeBase messageEnvelope, CancellationToken cancellationToken = default) =>
        StoreAsync(messageEnvelope.Message as IMessage
            ?? throw new InvalidOperationException("Integration envelope must contain an IMessage."), cancellationToken);

    public Task PublishAsync<TMessage>(TMessage message, string? exchangeOrTopic = null, string? queue = null,
        CancellationToken cancellationToken = default)
        where TMessage : class, IMessage => StoreAsync(message, cancellationToken);

    public Task PublishAsync<TMessage>(IMessageEnvelope<TMessage> messageEnvelope, string? exchangeOrTopic = null,
        string? queue = null, CancellationToken cancellationToken = default)
        where TMessage : class, IMessage => StoreAsync(messageEnvelope.Message, cancellationToken);
}
