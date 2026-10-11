using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Wolverine.Runtime;

namespace FoodDelivery.Api.Messaging;

public sealed class ModuleMessagingSession(IWolverineRuntime runtime) : DbTransactionInterceptor
{
    public MessageContext Context { get; private set; } = new(runtime);
    private DbTransaction? _transaction;
    private bool _consumer;

    public void BindConsumer(MessageContext context)
    {
        Context = context;
        _consumer = true;
    }

    public void Track(DbTransaction transaction)
    {
        if (!_consumer && _transaction is not null && !ReferenceEquals(_transaction, transaction)) Reset();
        _transaction = transaction;
    }

    public override async Task TransactionCommittedAsync(DbTransaction transaction,
        TransactionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        if (!_consumer && ReferenceEquals(_transaction, transaction))
        {
            try
            {
                await Context.FlushOutgoingMessagesAsync();
            }
            finally
            {
                Reset();
            }
        }
    }

    public override void TransactionCommitted(DbTransaction transaction, TransactionEndEventData eventData)
    {
        if (!_consumer && ReferenceEquals(_transaction, transaction))
        {
            try
            {
                Context.FlushOutgoingMessagesAsync().GetAwaiter().GetResult();
            }
            finally
            {
                Reset();
            }
        }
    }

    public override Task TransactionRolledBackAsync(DbTransaction transaction,
        TransactionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        if (!_consumer && ReferenceEquals(_transaction, transaction)) Reset();
        return Task.CompletedTask;
    }

    public override void TransactionRolledBack(DbTransaction transaction, TransactionEndEventData eventData)
    {
        if (!_consumer && ReferenceEquals(_transaction, transaction)) Reset();
    }

    private void Reset()
    {
        Context = new MessageContext(runtime);
        _transaction = null;
    }
}
