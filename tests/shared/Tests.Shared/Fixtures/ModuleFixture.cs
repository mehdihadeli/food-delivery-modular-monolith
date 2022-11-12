using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Command;
using BuildingBlocks.Abstractions.CQRS.Query;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Abstractions.Web;
using BuildingBlocks.Abstractions.Web.Module;
using BuildingBlocks.Core.Types;
using BuildingBlocks.Persistence.Mongo;
using Hypothesist;
using MediatR;
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
        Scope = serviceProvider.CreateAsyncScope();
    }

    public IServiceProvider ServiceProvider { get; }

    public IBus Bus => ServiceProvider.GetRequiredService<IBus>();
    public AsyncServiceScope Scope { get; }
    public IGatewayProcessor<TModule> GatewayProcessor { get; }

    public async Task AssertEventually(IProbe probe, int timeout)
    {
        await new Poller(timeout).CheckAsync(probe);
    }

    public async ValueTask ExecuteScopeAsync(Func<IServiceProvider, ValueTask> action)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        await action(scope.ServiceProvider);
    }

    public async ValueTask<T> ExecuteScopeAsync<T>(Func<IServiceProvider, ValueTask<T>> action)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();

        var result = await action(scope.ServiceProvider);

        return result;
    }

    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        return await ExecuteScopeAsync(async sp =>
        {
            var mediator = sp.GetRequiredService<IMediator>();

            return await mediator.Send(request);
        });
    }

    public async Task<TResponse> SendAsync<TResponse>(
        ICommand<TResponse> request,
        CancellationToken cancellationToken = default)
        where TResponse : notnull
    {
        return await ExecuteScopeAsync(async sp =>
        {
            var commandProcessor = sp.GetRequiredService<ICommandProcessor>();

            return await commandProcessor.SendAsync(request, cancellationToken);
        });
    }

    public async Task SendAsync<T>(T request, CancellationToken cancellationToken = default) where T : class, ICommand
    {
        await ExecuteScopeAsync(async sp =>
        {
            var commandProcessor = sp.GetRequiredService<ICommandProcessor>();

            return await commandProcessor.SendAsync(request, cancellationToken);
        });
    }

    public async Task<TResponse> QueryAsync<TResponse>(
        IQuery<TResponse> query,
        CancellationToken cancellationToken = default) where TResponse : class
    {
        return await ExecuteScopeAsync(async sp =>
        {
            var queryProcessor = sp.GetRequiredService<IQueryProcessor>();

            return await queryProcessor.SendAsync(query, cancellationToken);
        });
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
    public async ValueTask WaitUntilConditionMet(Func<Task<bool>> conditionToMet, int timeoutSecond = 60)
    {
        var startTime = DateTime.Now;
        var timeoutExpired = false;
        var meet = await conditionToMet.Invoke();
        while (!meet)
        {
            if (timeoutExpired)
                throw new TimeoutException("Condition not met for the test.");

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
        await WaitUntilConditionMet(async () =>
        {
            return await ExecuteScopeAsync(async sp =>
            {
                var messagePersistenceService = sp.GetService<IMessagePersistenceService>();
                Guard.Against.Null(messagePersistenceService, nameof(messagePersistenceService));

                var filter = await messagePersistenceService.GetByFilterAsync(x =>
                    x.DeliveryType == MessageDeliveryType.Outbox &&
                    TypeMapper.GetFullTypeName(typeof(TMessage)) == x.DataType);

                return filter.Any(x => x.MessageStatus == MessageStatus.Processed);
            });
        });
    }

    public async ValueTask ShouldProcessedPersistInternalCommand<TInternalCommand>()
        where TInternalCommand : class, IInternalCommand
    {
        await WaitUntilConditionMet(async () =>
        {
            return await ExecuteScopeAsync(async sp =>
            {
                var messagePersistenceService = sp.GetService<IMessagePersistenceService>();
                Guard.Against.Null(messagePersistenceService, nameof(messagePersistenceService));

                var filter = await messagePersistenceService.GetByFilterAsync(x =>
                    x.DeliveryType == MessageDeliveryType.Internal &&
                    TypeMapper.GetFullTypeName(typeof(TInternalCommand)) == x.DataType);

                var res = filter.Any(x => x.MessageStatus == MessageStatus.Processed);

                return res;
            });
        });
    }

    public async ValueTask DisposeAsync()
    {
        await Scope.DisposeAsync();
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
        await using var scope = ServiceProvider.CreateAsyncScope();
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
        await using var scope = ServiceProvider.CreateAsyncScope();
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
