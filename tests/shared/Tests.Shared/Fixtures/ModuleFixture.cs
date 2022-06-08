using BuildingBlocks.Abstractions.CQRS.Command;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Abstractions.Web;
using BuildingBlocks.Abstractions.Web.Module;
using BuildingBlocks.Core.Types;
using BuildingBlocks.Persistence.Mongo;
using Hypothesist;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.Shared.Probing;

namespace Tests.Shared.Fixtures;

public class ModuleFixture<TModule> : IAsyncDisposable
    where TModule : class, IModuleDefinition
{
    public ModuleFixture(IServiceProvider serviceProvider, IGatewayProcessor<TModule> gatewayProcessor)
    {
        ServiceProvider = serviceProvider;
        GatewayProcessor = gatewayProcessor;
        Scope = serviceProvider.CreateScope();
        MessagePersistenceService = Scope.ServiceProvider.GetRequiredService<IMessagePersistenceService>();
    }

    public IServiceProvider ServiceProvider { get; }

    public IMessagePersistenceService MessagePersistenceService { get; }

    public IBus Bus => ServiceProvider.GetRequiredService<IBus>();
    public IServiceScope Scope { get; }
    public IGatewayProcessor<TModule> GatewayProcessor { get; }

    public async Task AssertEventually(IProbe probe, int timeout)
    {
        await new Poller(timeout).CheckAsync(probe);
    }

    public async ValueTask ExecuteScopeAsync(Func<IServiceProvider, ValueTask> action)
    {
        using var scope = ServiceProvider.CreateScope();
        await action(scope.ServiceProvider);
    }

    public async ValueTask<T> ExecuteScopeAsync<T>(Func<IServiceProvider, ValueTask<T>> action)
    {
        using var scope = ServiceProvider.CreateScope();

        var result = await action(scope.ServiceProvider);

        return result;
    }

    public async ValueTask PublishMessageAsync<TMessage>(
        TMessage message,
        IDictionary<string, object?>? headers = null,
        CancellationToken cancellationToken = default)
        where
        TMessage : class, IMessage
    {
        await ExecuteScopeAsync(async sp =>
        {
            var bus = sp.GetRequiredService<IBus>();

            await bus.PublishAsync(message, headers, cancellationToken);
        });
    }

    // Ref: https://tech.energyhelpline.com/in-memory-testing-with-masstransit/
    public async ValueTask WaitUntilConditionMetOrTimedOut(Func<Task<bool>> conditionToMet, int timeoutSecond = 60)
    {
        var startTime = DateTime.Now;
        var timeoutExpired = false;
        var meet = await conditionToMet.Invoke();
        while (!meet && !timeoutExpired)
        {
            await Task.Delay(100);
            meet = await conditionToMet.Invoke();
            timeoutExpired = DateTime.Now - startTime > TimeSpan.FromSeconds(timeoutSecond);
        }
    }

    public async ValueTask<IHypothesis<object>> ShouldPublish(Predicate<object>? match = null)
    {
        var hypothesis = Hypothesis
            .For<object>()
            .Any(match ?? (_ => true));

        Bus.MessagePublished += async message =>
        {
            if (message.GetType() == typeof(object))
                await hypothesis.Test(message);
        };

        return hypothesis;
    }

    public async ValueTask<IHypothesis<TMessage>> ShouldPublish<TMessage>(Predicate<TMessage>? match = null)
        where TMessage : class, IMessage
    {
        var hypothesis = Hypothesis
            .For<TMessage>()
            .Any(match ?? (_ => true));

        Bus.MessagePublished += async message =>
        {
            if (message.GetType() == typeof(TMessage) && message is TMessage messageData)
                await hypothesis.Test(messageData);
        };

        return hypothesis;
    }

    public async ValueTask<IHypothesis<TMessage>> ShouldConsume<TMessage>(Predicate<TMessage>? match = null)
        where TMessage : class, IMessage
    {
        var hypothesis = Hypothesis
            .For<TMessage>()
            .Any(match ?? (_ => true));

        Bus.MessageConsumed += async (message, _) =>
        {
            if (message.GetType() == typeof(TMessage) && message is TMessage messageData)
            {
                await hypothesis.Test(messageData);
            }
        };

        return hypothesis;
    }

    public async ValueTask<IHypothesis<TMessage>> ShouldConsume<TMessage, TMessageHandler>(
        Predicate<TMessage>? match = null)
        where TMessage : class, IMessage
        where TMessageHandler : class, IMessageHandler<TMessage>
    {
        var hypothesis = Hypothesis
            .For<TMessage>()
            .Any(match ?? (_ => true));

        Bus.MessageConsumed += async (message, handlerType) =>
        {
            if (message.GetType() == typeof(TMessage) && message is TMessage messageData &&
                typeof(TMessageHandler).IsAssignableTo(handlerType))
            {
                await hypothesis.Test(messageData);
            }
        };

        return hypothesis;
    }

    public async ValueTask<IHypothesis<TMessage>> ShouldConsumeWithNewConsumer<TMessage>(
        Predicate<TMessage>? match = null)
        where TMessage : class, IMessage
    {
        var hypothesis = Hypothesis
            .For<TMessage>()
            .Any(match ?? (_ => true));

        // Bus.Consume(hypothesis.AsMessageHandler());

        Bus.Consume<TMessage>(async
            (consumeContext, ct) =>
        {
            await hypothesis.Test(consumeContext.Message);
        });

        return hypothesis;
    }

    public async ValueTask<IHypothesis<TMessage>> ShouldConsumeWithNewConsumer<TMessage, TMessageHandler>(
        Predicate<TMessage>? match = null)
        where TMessage : class, IMessage
        where TMessageHandler : IMessageHandler<TMessage>
    {
        var hypothesis = Hypothesis
            .For<TMessage>()
            .Any(match ?? (_ => true));

        Bus.Consume(hypothesis.AsMessageHandler<TMessage, TMessageHandler>(ServiceProvider));

        return hypothesis;
    }

    public async ValueTask ShouldProcessedOutboxPersistMessage<TMessage>()
        where TMessage : class, IMessage
    {
        await WaitUntilConditionMetOrTimedOut(async () =>
        {
            var filter = await MessagePersistenceService.GetByFilterAsync(x =>
                x.DeliveryType == MessageDeliveryType.Outbox &&
                TypeMapper.GetTypeName(typeof(TMessage)) == x.DataType);

            return filter.Any(x => x.MessageStatus == MessageStatus.Processed);
        });
    }

    public async ValueTask ShouldProcessedPersistInternalCommand<TInternalCommand>()
        where TInternalCommand : class, IInternalCommand
    {
        await WaitUntilConditionMetOrTimedOut(async () =>
        {
            var filter = await MessagePersistenceService.GetByFilterAsync(x =>
                x.DeliveryType == MessageDeliveryType.Internal &&
                TypeMapper.GetTypeName(typeof(TInternalCommand)) == x.DataType);

            return filter.Any(x => x.MessageStatus == MessageStatus.Processed);
        });
    }


    public ValueTask DisposeAsync()
    {
        Scope.Dispose();

        return ValueTask.CompletedTask;
    }
}

public class ModuleFixture<TModule, TContext> : ModuleFixture<TModule>
    where TModule : class, IModuleDefinition
    where TContext : DbContext
{
    public ModuleFixture(IServiceProvider serviceProvider, IGatewayProcessor<TModule> gatewayProcessor) : base(
        serviceProvider, gatewayProcessor)
    {
    }

    public async Task ExecuteTxContextAsync(Func<IServiceProvider, TContext, ValueTask> action)
    {
        using var scope = ServiceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            try
            {
                await dbContext.Database.BeginTransactionAsync();

                await action(scope.ServiceProvider, dbContext);

                await dbContext.Database.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                dbContext.Database?.RollbackTransactionAsync();
                throw;
            }
        });
    }

    public async Task<T> ExecuteTxContextAsync<T>(Func<IServiceProvider, TContext, ValueTask<T>> action)
    {
        using var scope = ServiceProvider.CreateScope();
        //https://weblogs.asp.net/dixin/entity-framework-core-and-linq-to-entities-7-data-changes-and-transactions
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            try
            {
                await dbContext.Database.BeginTransactionAsync();

                var result = await action(scope.ServiceProvider, dbContext);

                await dbContext.Database.CommitTransactionAsync();

                return result;
            }
            catch (Exception ex)
            {
                dbContext.Database?.RollbackTransactionAsync();
                throw;
            }
        });
    }

    public ValueTask ExecuteContextAsync(Func<TContext, ValueTask> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TContext>()));

    public ValueTask ExecuteContextAsync(Func<TContext, ICommandProcessor, ValueTask> action)
        => ExecuteScopeAsync(sp =>
            action(sp.GetRequiredService<TContext>(), sp.GetRequiredService<ICommandProcessor>()));

    public ValueTask<T> ExecuteContextAsync<T>(Func<TContext, ValueTask<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TContext>()));

    public ValueTask<T> ExecuteContextAsync<T>(Func<TContext, ICommandProcessor, ValueTask<T>> action)
        => ExecuteScopeAsync(sp =>
            action(sp.GetRequiredService<TContext>(), sp.GetRequiredService<ICommandProcessor>()));

    public async ValueTask<int> InsertAsync<T>(params T[] entities) where T : class
    {
        return await ExecuteContextAsync(async db =>
        {
            foreach (var entity in entities)
            {
                db.Set<T>().Add(entity);
            }

            return await db.SaveChangesAsync();
        });
    }

    public async ValueTask<int> InsertAsync<TEntity>(TEntity entity) where TEntity : class
    {
        return await ExecuteContextAsync(async db =>
        {
            db.Set<TEntity>().Add(entity);

            return await db.SaveChangesAsync();
        });
    }

    public async ValueTask<int> InsertAsync<TEntity, TEntity2>(TEntity entity, TEntity2 entity2)
        where TEntity : class
        where TEntity2 : class
    {
        return await ExecuteContextAsync(async db =>
        {
            db.Set<TEntity>().Add(entity);
            db.Set<TEntity2>().Add(entity2);

            return await db.SaveChangesAsync();
        });
    }

    public async ValueTask<int> InsertAsync<TEntity, TEntity2, TEntity3>(TEntity entity, TEntity2 entity2, TEntity3
        entity3)
        where TEntity : class
        where TEntity2 : class
        where TEntity3 : class
    {
        return await ExecuteContextAsync(async db =>
        {
            db.Set<TEntity>().Add(entity);
            db.Set<TEntity2>().Add(entity2);
            db.Set<TEntity3>().Add(entity3);

            return await db.SaveChangesAsync();
        });
    }

    public async ValueTask<int> InsertAsync<TEntity, TEntity2, TEntity3, TEntity4>(TEntity entity, TEntity2 entity2,
        TEntity3 entity3, TEntity4 entity4)
        where TEntity : class
        where TEntity2 : class
        where TEntity3 : class
        where TEntity4 : class
    {
        return await ExecuteContextAsync(async db =>
        {
            db.Set<TEntity>().Add(entity);
            db.Set<TEntity2>().Add(entity2);
            db.Set<TEntity3>().Add(entity3);
            db.Set<TEntity4>().Add(entity4);

            return await db.SaveChangesAsync();
        });
    }

    public ValueTask<T?> FindAsync<T>(object id) where T : class
    {
        return ExecuteContextAsync(db => db.Set<T>().FindAsync(id));
    }
}

public class ModuleFixture<TModule, TWContext, TRContext> : ModuleFixture<TModule, TWContext>
    where TModule : class, IModuleDefinition
    where TWContext : DbContext
    where TRContext : MongoDbContext
{
    public ModuleFixture(IServiceProvider serviceProvider, IGatewayProcessor<TModule> gatewayProcessor)
        : base(serviceProvider, gatewayProcessor)
    {
    }

    public ValueTask ExecuteReadContextAsync(Func<TRContext, ValueTask> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TRContext>()));

    public ValueTask ExecuteReadContextAsync(Func<TRContext, ICommandProcessor, ValueTask> action)
        => ExecuteScopeAsync(sp =>
            action(sp.GetRequiredService<TRContext>(), sp.GetRequiredService<ICommandProcessor>()));

    public ValueTask<T> ExecuteReadContextAsync<T>(Func<TRContext, ValueTask<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TRContext>()));

    public ValueTask<T> ExecuteReadContextAsync<T>(Func<TRContext, ICommandProcessor, ValueTask<T>> action)
        => ExecuteScopeAsync(sp =>
            action(sp.GetRequiredService<TRContext>(), sp.GetRequiredService<ICommandProcessor>()));
}
