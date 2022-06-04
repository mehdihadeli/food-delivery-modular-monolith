using BuildingBlocks.Abstractions.CQRS.Command;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Abstractions.Web;
using BuildingBlocks.Abstractions.Web.Module;
using BuildingBlocks.Persistence.EfCore.Postgres;
using BuildingBlocks.Persistence.Mongo;
using Hypothesist;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mongo2Go;
using Npgsql;
using Respawn;
using Respawn.Graph;

namespace Tests.Shared.Fixtures;

public class ModuleFixture<TModule, TWContext> : IDisposable
    where TModule : class, IModuleDefinition
    where TWContext : DbContext
{
    private readonly Checkpoint _checkpoint;
    private readonly MongoDbRunner _mongoRunner;

    public ModuleFixture(IServiceProvider serviceProvider, IGatewayProcessor<TModule> gatewayProcessor, string name)
    {
        ServiceProvider = serviceProvider;
        Name = name;
        GatewayProcessor = gatewayProcessor;
        _checkpoint = new Checkpoint
        {
            // SchemasToInclude = new[] {"public"},
            DbAdapter = DbAdapter.Postgres, TablesToIgnore = new List<Table> {new("__EFMigrationsHistory"),}.ToArray()
        };
        _mongoRunner = MongoDbRunner.Start();
        var mongoOptions = serviceProvider.GetService<IOptions<MongoOptions>>();
        if (mongoOptions is { })
            mongoOptions.Value.ConnectionString = _mongoRunner.ConnectionString;

        SeedData();
    }

    private void SeedData()
    {
        using (var scope = ServiceProvider.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<IDbFacadeResolver>();
            var seeders = scope.ServiceProvider.GetServices<IDataSeeder>();
            ctx.Database.Migrate();

            foreach (var seeder in seeders)
            {
                seeder.SeedAllAsync().GetAwaiter().GetResult();
            }
        }
    }

    public IServiceProvider ServiceProvider { get; }

    public string Name { get; }

    public IBus Bus => ServiceProvider.GetRequiredService<IBus>();

    public IGatewayProcessor<TModule> GatewayProcessor { get; }

    private async Task ResetState()
    {
        try
        {
            var postgresOptions = ServiceProvider.GetService<IOptions<PostgresOptions>>();
            if (postgresOptions is { } && !string.IsNullOrEmpty(postgresOptions.Value.ConnectionString))
            {
                await using var conn = new NpgsqlConnection(postgresOptions.Value.ConnectionString);
                await conn.OpenAsync();

                await _checkpoint.Reset(conn);
            }
        }
        catch (Exception ex)
        {
            // ignored
        }
    }

    public async Task ExecuteScopeAsync(Func<IServiceProvider, Task> action)
    {
        using var scope = ServiceProvider.CreateScope();
        await action(scope.ServiceProvider);
    }

    public async Task<T> ExecuteScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        using var scope = ServiceProvider.CreateScope();

        var result = await action(scope.ServiceProvider);

        return result;
    }

    public async Task ExecuteTxWriteContextAsync(Func<IServiceProvider, TWContext, Task> action)
    {
        using var scope = ServiceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TWContext>();
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

    public async Task<T> ExecuteTxWriteContextAsync<T>(Func<IServiceProvider, TWContext, Task<T>> action)
    {
        using var scope = ServiceProvider.CreateScope();
        //https://weblogs.asp.net/dixin/entity-framework-core-and-linq-to-entities-7-data-changes-and-transactions
        var dbContext = scope.ServiceProvider.GetRequiredService<TWContext>();
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

    public Task ExecuteWriteContextAsync(Func<TWContext, Task> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TWContext>()));

    public Task ExecuteWriteContextAsync(Func<TWContext, ValueTask> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TWContext>()).AsTask());

    public Task ExecuteWriteContextAsync(Func<TWContext, ICommandProcessor, Task> action)
        => ExecuteScopeAsync(sp =>
            action(sp.GetRequiredService<TWContext>(), sp.GetRequiredService<ICommandProcessor>()));

    public Task<T> ExecuteWriteContextAsync<T>(Func<TWContext, Task<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TWContext>()));

    public Task<T> ExecuteWriteContextAsync<T>(Func<TWContext, ValueTask<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TWContext>()).AsTask());

    public Task<T> ExecuteWriteContextAsync<T>(Func<TWContext, ICommandProcessor, Task<T>> action)
        => ExecuteScopeAsync(sp =>
            action(sp.GetRequiredService<TWContext>(), sp.GetRequiredService<ICommandProcessor>()));

    public Task InsertAsync<T>(params T[] entities) where T : class
    {
        return ExecuteWriteContextAsync(db =>
        {
            foreach (var entity in entities)
            {
                db.Set<T>().Add(entity);
            }

            return db.SaveChangesAsync();
        });
    }

    public Task InsertAsync<TEntity>(TEntity entity) where TEntity : class
    {
        return ExecuteWriteContextAsync(db =>
        {
            db.Set<TEntity>().Add(entity);

            return db.SaveChangesAsync();
        });
    }

    public Task InsertAsync<TEntity, TEntity2>(TEntity entity, TEntity2 entity2)
        where TEntity : class
        where TEntity2 : class
    {
        return ExecuteWriteContextAsync(db =>
        {
            db.Set<TEntity>().Add(entity);
            db.Set<TEntity2>().Add(entity2);

            return db.SaveChangesAsync();
        });
    }

    public Task InsertAsync<TEntity, TEntity2, TEntity3>(TEntity entity, TEntity2 entity2, TEntity3 entity3)
        where TEntity : class
        where TEntity2 : class
        where TEntity3 : class
    {
        return ExecuteWriteContextAsync(db =>
        {
            db.Set<TEntity>().Add(entity);
            db.Set<TEntity2>().Add(entity2);
            db.Set<TEntity3>().Add(entity3);

            return db.SaveChangesAsync();
        });
    }

    public Task InsertAsync<TEntity, TEntity2, TEntity3, TEntity4>(TEntity entity, TEntity2 entity2,
        TEntity3 entity3, TEntity4 entity4)
        where TEntity : class
        where TEntity2 : class
        where TEntity3 : class
        where TEntity4 : class
    {
        return ExecuteWriteContextAsync(db =>
        {
            db.Set<TEntity>().Add(entity);
            db.Set<TEntity2>().Add(entity2);
            db.Set<TEntity3>().Add(entity3);
            db.Set<TEntity4>().Add(entity4);

            return db.SaveChangesAsync();
        });
    }

    public Task<T?> FindWriteAsync<T>(object id) where T : class
    {
        return ExecuteWriteContextAsync(db => db.Set<T>().FindAsync(id).AsTask());
    }

    public Task PublishMessageAsync<TMessage>(TMessage message, IDictionary<string, object?>? headers = null) where
        TMessage : class, IMessage
    {
        return ExecuteScopeAsync(sp =>
        {
            var bus = sp.GetRequiredService<IBus>();

            return bus.PublishAsync(message, headers, CancellationToken.None);
        });
    }

    // Ref: https://tech.energyhelpline.com/in-memory-testing-with-masstransit/
    public async Task WaitUntilConditionMetOrTimedOut(Func<bool> conditionToMet)
    {
        var meet = conditionToMet.Invoke();
        while (!meet)
        {
            await Task.Delay(100);
            meet = conditionToMet.Invoke();
        }
    }

    public async Task<IHypothesis<object>> ShouldPublish(Predicate<object>? match = null)
    {
        var hypothesis = Hypothesis
            .For<object>()
            .Any(match ?? (_ => true));

        Bus.RemoveAllConsume();

        Bus.MessagePublished += message =>
        {
            if (message.GetType() == typeof(object))
                hypothesis.Test(message);
        };

        return hypothesis;
    }

    public async Task<IHypothesis<TMessage>> ShouldPublish<TMessage>(Predicate<TMessage>? match = null)
        where TMessage : class, IMessage
    {
        var hypothesis = Hypothesis
            .For<TMessage>()
            .Any(match ?? (_ => true));

        Bus.RemoveAllConsume();

        Bus.MessagePublished += message =>
        {
            if (message.GetType() == typeof(TMessage) && message is TMessage messageData)
                hypothesis.Test(messageData);
        };

        return hypothesis;
    }

    public async Task<IHypothesis<TMessage>> ShouldConsume<TMessage>(Predicate<TMessage>? match = null)
        where TMessage : class, IMessage
    {
        var hypothesis = Hypothesis
            .For<TMessage>()
            .Any(match ?? (_ => true));

        Bus.MessageConsumed += (message, _) =>
        {
            if (message.GetType() == typeof(TMessage) && message is TMessage messageData)
                hypothesis.Test(messageData);
        };

        return hypothesis;
    }

    public async Task<IHypothesis<TMessage>> ShouldConsumeWithNewConsumer<TMessage>(Predicate<TMessage>? match = null)
        where TMessage : class, IMessage
    {
        var hypothesis = Hypothesis
            .For<TMessage>()
            .Any(match ?? (_ => true));

        Bus.RemoveAllConsume();

        // Bus.Consume(hypothesis.AsMessageHandler());

        Bus.Consume<TMessage>(async
            (consumeContext, ct) =>
        {
            await hypothesis.Test(consumeContext.Message);
        });

        return hypothesis;
    }

    public async Task<IHypothesis<TMessage>> ShouldConsume<TMessage, TMessageHandler>(
        Predicate<TMessage>? match = null)
        where TMessage : class, IMessage
        where TMessageHandler : class, IMessageHandler<TMessage>
    {
        var hypothesis = Hypothesis
            .For<TMessage>()
            .Any(match ?? (_ => true));

        Bus.MessageConsumed += (message, handlerType) =>
        {
            if (message.GetType() == typeof(TMessage) && message is TMessage messageData &&
                typeof(TMessageHandler).IsAssignableTo(handlerType))
                hypothesis.Test(messageData);
        };

        return hypothesis;
    }

    public void Dispose()
    {
        _mongoRunner.Dispose();
        ResetState().GetAwaiter().GetResult();
    }
}

public class ModuleFixture<TModule, TWContext, TRContext> : ModuleFixture<TModule, TWContext>
    where TModule : class, IModuleDefinition
    where TWContext : DbContext
    where TRContext : MongoDbContext
{
    public ModuleFixture(IServiceProvider serviceProvider, IGatewayProcessor<TModule> gatewayProcessor, string name)
        : base(serviceProvider, gatewayProcessor, name)
    {
    }

    public Task ExecuteReadContextAsync(Func<TRContext, Task> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TRContext>()));

    public Task ExecuteReadContextAsync(Func<TRContext, ValueTask> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TRContext>()).AsTask());

    public Task ExecuteReadContextAsync(Func<TRContext, ICommandProcessor, Task> action)
        => ExecuteScopeAsync(sp =>
            action(sp.GetRequiredService<TRContext>(), sp.GetRequiredService<ICommandProcessor>()));

    public Task<T> ExecuteReadContextAsync<T>(Func<TRContext, Task<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TRContext>()));

    public Task<T> ExecuteReadContextAsync<T>(Func<TRContext, ValueTask<T>> action)
        => ExecuteScopeAsync(sp => action(sp.GetRequiredService<TRContext>()).AsTask());

    public Task<T> ExecuteReadContextAsync<T>(Func<TRContext, ICommandProcessor, Task<T>> action)
        => ExecuteScopeAsync(sp =>
            action(sp.GetRequiredService<TRContext>(), sp.GetRequiredService<ICommandProcessor>()));
}
