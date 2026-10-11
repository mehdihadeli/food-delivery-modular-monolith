using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Squirrel.Abstractions.Commands;
using Squirrel.Abstractions.Queries;
using Squirrel.Persistence.Mongo;
using Xunit.Sdk;
using Xunit.v3;

namespace Tests.Shared.Fixtures;

/// <summary>
/// Adds Mongo read-model helpers on top of <see cref="SharedFixtureWithEfCore{TEntryPoint, TEfCoreDbContext}"/>
/// so tests can run actions against the read <typeparamref name="TMongoDbContext"/>.
/// </summary>
public class SharedFixtureWithEfCoreAndMongo<TEntryPoint, TEfCoreDbContext, TMongoDbContext>
    : SharedFixtureWithEfCore<TEntryPoint, TEfCoreDbContext>
    where TEfCoreDbContext : DbContext
    where TMongoDbContext : MongoDbContext
    where TEntryPoint : class
{
    public Task ExecuteMongoDbContextAsync(Func<IServiceProvider, TMongoDbContext, Task> action) =>
        ExecuteScopeAsync(sp => action(sp, sp.GetRequiredService<TMongoDbContext>()));

    public Task<T> ExecuteMongoDbContextAsync<T>(Func<IServiceProvider, TMongoDbContext, Task<T>> action) =>
        ExecuteScopeAsync(sp => action(sp, sp.GetRequiredService<TMongoDbContext>()));

    public Task ExecuteMongoDbContextAsync(Func<TMongoDbContext, Task> action) =>
        ExecuteScopeAsync(sp => action(sp.GetRequiredService<TMongoDbContext>()));

    public Task ExecuteMongoDbContextAsync(Func<TMongoDbContext, ICommandBus, Task> action) =>
        ExecuteScopeAsync(sp =>
            action(sp.GetRequiredService<TMongoDbContext>(), sp.GetRequiredService<ICommandBus>()));

    public Task<T> ExecuteMongoDbContextAsync<T>(Func<TMongoDbContext, ICommandBus, Task<T>> action) =>
        ExecuteScopeAsync(sp =>
            action(sp.GetRequiredService<TMongoDbContext>(), sp.GetRequiredService<ICommandBus>()));

    public Task ExecuteMongoDbContextAsync<T>(Func<TMongoDbContext, IQueryBus, Task<T>> action) =>
        ExecuteScopeAsync(sp =>
            action(sp.GetRequiredService<TMongoDbContext>(), sp.GetRequiredService<IQueryBus>()));

    public Task<T> ExecuteMongoDbContextAsync<T>(Func<TMongoDbContext, Task<T>> action) =>
        ExecuteScopeAsync(sp => action(sp.GetRequiredService<TMongoDbContext>()));

    public async Task InsertMongoDbContextAsync<T>(params T[] entities)
        where T : class
    {
        await ExecuteMongoDbContextAsync(async db =>
        {
            await db.GetCollection<T>().InsertManyAsync(entities.ToList());
        });
    }

    public SharedFixtureWithEfCoreAndMongo(IMessageSink messageSink)
        : base(messageSink)
    {
        messageSink.OnMessage(new DiagnosticMessage("Constructing SharedFixtureWithEfCoreAndMongo ..."));
    }
}
