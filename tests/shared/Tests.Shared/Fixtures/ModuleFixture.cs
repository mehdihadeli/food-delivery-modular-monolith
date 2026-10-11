using Squirrel.Abstractions.Commands;
using Squirrel.Abstractions.Queries;
using Squirrel.Abstractions.Messages;
using Squirrel.Abstractions.Web;
using Squirrel.Abstractions.Web.Module;
using Squirrel.Persistence.Mongo;
using Hypothesist;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tests.Shared.Probing;
using Wolverine;
using Wolverine.Tracking;
using ICommandBus = Squirrel.Abstractions.Commands.ICommandBus;
using IExternalEventBus = Squirrel.Abstractions.Messages.IExternalEventBus;
using IMessage = Squirrel.Abstractions.Messages.IMessage;
using IQueryBus = Squirrel.Abstractions.Queries.IQueryBus;
using IMessageContext = Wolverine.IMessageContext;

namespace Tests.Shared.Fixtures;

public class ModuleFixture<TModule> : IAsyncDisposable
    where TModule : class, IModuleConfiguration
{
    private readonly List<Func<ITrackedSession, Task>> _trackingAssertions = [];

    public ModuleFixture(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        Scope = serviceProvider.CreateAsyncScope();
    }

    public IServiceProvider ServiceProvider { get; }

    public IExternalEventBus Bus => ServiceProvider.GetRequiredService<IExternalEventBus>();
    public AsyncServiceScope Scope { get; }

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
        return await action(scope.ServiceProvider);
    }

    public async Task<TResponse> SendAsync<TResponse>(
        ICommand<TResponse> request,
        CancellationToken cancellationToken = default)
        where TResponse : notnull
    {
        return await ExecuteScopeAsync(async sp =>
        {
            var commandBus = sp.GetRequiredService<ICommandBus>();
            return await commandBus.SendAsync(request, cancellationToken);
        });
    }

    public async Task SendAsync<T>(T request, CancellationToken cancellationToken = default)
        where T : class, ICommand
    {
        await ExecuteScopeAsync(async sp =>
        {
            var commandBus = sp.GetRequiredService<ICommandBus>();
            await commandBus.SendAsync(request, cancellationToken);
        });
    }

    public async Task<TResponse> QueryAsync<TResponse>(
        IQuery<TResponse> query,
        CancellationToken cancellationToken = default)
        where TResponse : class
    {
        return await ExecuteScopeAsync(async sp =>
        {
            var queryBus = sp.GetRequiredService<IQueryBus>();
            return await queryBus.SendAsync(query, cancellationToken);
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
            var bus = sp.GetRequiredService<IExternalEventBus>();
                var trackedSession = await sp.TrackActivity()
                    .ExecuteAndWaitAsync((Func<IMessageContext, Task>)(async _ =>
                        await bus.PublishAsync(message, cancellationToken)));

                foreach (var assertion in _trackingAssertions.ToArray())
                {
                    await assertion(trackedSession);
                }

                _trackingAssertions.Clear();
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

        _trackingAssertions.Add(session => TestTrackedMessagesAsync(session, MessageEventType.Sent, hypothesis, match));

        return hypothesis;
    }

    public async ValueTask<IHypothesis<TMessage>> ShouldPublish<TMessage>(Predicate<TMessage>? match = null)
        where TMessage : class, IMessage
    {
        var hypothesis = Hypothesis
            .For<TMessage>()
            .Any(match ?? (_ => true));

        _trackingAssertions.Add(session => TestTrackedMessagesAsync(session, MessageEventType.Sent, hypothesis, match));

        return hypothesis;
    }

    public async ValueTask<IHypothesis<TMessage>> ShouldConsume<TMessage>(Predicate<TMessage>? match = null)
        where TMessage : class, IMessage
    {
        var hypothesis = Hypothesis
            .For<TMessage>()
            .Any(match ?? (_ => true));

        _trackingAssertions.Add(session => TestTrackedMessagesAsync(session, MessageEventType.MessageSucceeded, hypothesis, match));

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

        _trackingAssertions.Add(session => TestTrackedMessagesAsync(session, MessageEventType.MessageSucceeded, hypothesis, match));

        return hypothesis;
    }

    public async ValueTask<IHypothesis<TMessage>> ShouldConsumeWithNewConsumer<TMessage>(
        Predicate<TMessage>? match = null)
        where TMessage : class, IMessage
    {
        var hypothesis = Hypothesis
            .For<TMessage>()
            .Any(match ?? (_ => true));

        _trackingAssertions.Add(session => TestTrackedMessagesAsync(session, MessageEventType.MessageSucceeded, hypothesis, match));

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

        _trackingAssertions.Add(session => TestTrackedMessagesAsync(session, MessageEventType.MessageSucceeded, hypothesis, match));

        return hypothesis;
    }

    private static async Task TestTrackedMessagesAsync<TMessage>(
        ITrackedSession session,
        MessageEventType eventType,
        IHypothesis<TMessage> hypothesis,
        Predicate<TMessage>? match)
        where TMessage : class
    {
        foreach (var envelope in session.FindEnvelopesWithMessageType<TMessage>(eventType))
        {
            if (envelope.Message is TMessage message && (match is null || match(message)))
            {
                await hypothesis.Test(message);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        await Scope.DisposeAsync();
    }
}

public class ModuleFixture<TModule, TContext> : ModuleFixture<TModule>
    where TModule : class, IModuleConfiguration
    where TContext : DbContext
{
    public ModuleFixture(IServiceProvider serviceProvider) : base(serviceProvider)
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

    public ValueTask ExecuteContextAsync(Func<TContext, ICommandBus, ValueTask> action)
        => ExecuteScopeAsync(sp =>
            action(sp.GetRequiredService<TContext>(), sp.GetRequiredService<ICommandBus>()));

    public ValueTask<T> ExecuteContextAsync<T>(Func<TContext, ValueTask<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TContext>()));

    public ValueTask<T> ExecuteContextAsync<T>(Func<TContext, ICommandBus, ValueTask<T>> action)
        => ExecuteScopeAsync(sp =>
            action(sp.GetRequiredService<TContext>(), sp.GetRequiredService<ICommandBus>()));

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
    where TModule : class, IModuleConfiguration
    where TWContext : DbContext
    where TRContext : MongoDbContext
{
    public ModuleFixture(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    public ValueTask ExecuteReadContextAsync(Func<TRContext, ValueTask> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TRContext>()));

    public ValueTask ExecuteReadContextAsync(Func<TRContext, ICommandBus, ValueTask> action)
        => ExecuteScopeAsync(sp =>
            action(sp.GetRequiredService<TRContext>(), sp.GetRequiredService<ICommandBus>()));

    public ValueTask<T> ExecuteReadContextAsync<T>(Func<TRContext, ValueTask<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TRContext>()));

    public ValueTask<T> ExecuteReadContextAsync<T>(Func<TRContext, ICommandBus, ValueTask<T>> action)
        => ExecuteScopeAsync(sp =>
            action(sp.GetRequiredService<TRContext>(), sp.GetRequiredService<ICommandBus>()));
}
