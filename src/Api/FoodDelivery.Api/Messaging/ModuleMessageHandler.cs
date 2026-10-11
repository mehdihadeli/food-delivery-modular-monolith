using Microsoft.Extensions.DependencyInjection;
using Squirrel.Abstractions.Messages;
using Squirrel.Abstractions.Web.Module;
using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore.Internals;
using Wolverine.Runtime;

namespace FoodDelivery.Api.Messaging;

public sealed class ModuleMessageHandler<TMessage, THandler>(IEnumerable<ICompositionRoot> moduleRoots)
    where TMessage : class, IMessage
    where THandler : class, IMessageHandler<TMessage>
{
    public async Task Handle(TMessage message, Wolverine.IMessageContext messaging, CancellationToken cancellationToken)
    {
        var root = moduleRoots.Single(module =>
            module.ModuleDefinition.GetType().Assembly == typeof(THandler).Assembly);
        await using var scope = root.ServiceProvider.CreateAsyncScope();
        var strategyDatabase = scope.ServiceProvider.GetRequiredService<ModuleMessagingDatabase>();
        var context = messaging as MessageContext
            ?? throw new InvalidOperationException("Module handler requires Wolverine runtime message context.");
        await strategyDatabase.Context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var attemptScope = root.ServiceProvider.CreateAsyncScope();
            var database = attemptScope.ServiceProvider.GetRequiredService<ModuleMessagingDatabase>();
            attemptScope.ServiceProvider.GetRequiredService<ModuleMessagingSession>().BindConsumer(context);
            await using var transaction = await database.Context.Database.BeginTransactionAsync(cancellationToken);
            var envelopes = new EfCoreEnvelopeTransaction(database.Context, context);
            await context.EnlistInOutboxAsync(envelopes);
            try
            {
                var handler = ActivatorUtilities.GetServiceOrCreateInstance<THandler>(attemptScope.ServiceProvider);
                await handler.Handle(message, cancellationToken);
                await database.Context.SaveChangesAsync(cancellationToken);
                await envelopes.CommitAsync(cancellationToken);
            }
            catch
            {
                await context.ClearAllAsync();
                throw;
            }
        });
    }
}
