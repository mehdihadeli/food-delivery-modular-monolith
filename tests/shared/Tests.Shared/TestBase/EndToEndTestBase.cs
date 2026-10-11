using Microsoft.EntityFrameworkCore;
using Squirrel.Persistence.Mongo;
using Tests.Shared.Fixtures;

namespace Tests.Shared.TestBase;

/// <summary>
/// Base class for end-to-end tests that drive the API over HTTP using the shared fixture.
/// </summary>
public class EndToEndTest<TEntryPoint>(SharedFixture<TEntryPoint> sharedFixture)
    : IntegrationTest<TEntryPoint>(sharedFixture)
    where TEntryPoint : class;

/// <summary>
/// End-to-end test base with an EF Core write context available to assert persisted state.
/// </summary>
public abstract class EndToEndTestBase<TEntryPoint, TContext>(
    SharedFixtureWithEfCore<TEntryPoint, TContext> sharedFixture
) : IntegrationTestBase<TEntryPoint, TContext>(sharedFixture)
    where TEntryPoint : class
    where TContext : DbContext
{
    public new SharedFixtureWithEfCore<TEntryPoint, TContext> SharedFixture { get; } = sharedFixture;
}

/// <summary>
/// End-to-end test base for read/write module split (EF Core write + Mongo read).
/// </summary>
public abstract class EndToEndTestBase<TEntryPoint, TWContext, TRContext>(
    SharedFixtureWithEfCoreAndMongo<TEntryPoint, TWContext, TRContext> sharedFixture
) : IntegrationTestBase<TEntryPoint, TWContext, TRContext>(sharedFixture)
    where TEntryPoint : class
    where TWContext : DbContext
    where TRContext : MongoDbContext
{
    public new SharedFixtureWithEfCoreAndMongo<TEntryPoint, TWContext, TRContext> SharedFixture { get; } =
        sharedFixture;
}
