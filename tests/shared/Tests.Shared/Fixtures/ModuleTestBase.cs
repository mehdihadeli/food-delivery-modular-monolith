using System.Net.Http.Headers;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Abstractions.Web;
using BuildingBlocks.Abstractions.Web.Module;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Persistence.EfCore.Postgres;
using BuildingBlocks.Persistence.Mongo;
using BuildingBlocks.Web.Module;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mongo2Go;
using Npgsql;
using Respawn;
using Respawn.Graph;
using Tests.Shared.Mocks;
using Tests.Shared.Mocks.Builders;
using Xunit.Abstractions;

namespace Tests.Shared.Fixtures;

public class ModuleTestBase<TEntryPoint, TModule> :
    IClassFixture<IntegrationTestFixture<TEntryPoint>>,
    IAsyncLifetime
    where TModule : class, IModuleDefinition
    where TEntryPoint : class
{
    private readonly IntegrationTestFixture<TEntryPoint> _integrationTestFixture;
    private readonly Checkpoint _checkpoint;
    private readonly MongoDbRunner _mongoRunner;

    public ModuleTestBase(IntegrationTestFixture<TEntryPoint> integrationTestFixture, ITestOutputHelper outputHelper)
    {
        _integrationTestFixture = integrationTestFixture;
        integrationTestFixture.RegisterTestServices(RegisterTestsServices);
        ModuleHook.ModuleServicesConfigured += RegisterModulesTestsServices;
        integrationTestFixture.SetOutputHelper(outputHelper);

        Logger = integrationTestFixture.ServiceProvider
            .GetRequiredService<ILogger<IntegrationTestFixture<TEntryPoint>>>();

        ModuleFixture = new ModuleFixture<TModule>(
            CompositionRootRegistry.GetByModule<TModule>()!.ServiceProvider,
            integrationTestFixture.ServiceProvider.GetRequiredService<IGatewayProcessor<TModule>>());

        Scope = ModuleFixture.Scope;

        AdminClient = integrationTestFixture.CreateClient();

        GuestClient = integrationTestFixture.CreateClient();

        UserClient = integrationTestFixture.CreateClient();

        var admin = new LoginRequestBuilder().Build();
        var user = new LoginRequestBuilder()
            .WithUserNameOrEmail(Constants.Users.User.UserName)
            .WithPassword(Constants.Users.User.Password)
            .Build();

        var adminLoginResult =
            GuestClient.PostAsJsonAsync<LoginUserRequestMock, LoginResponseMock>(Constants.LoginApi, admin)
                .GetAwaiter().GetResult();

        var userLoginResult =
            GuestClient.PostAsJsonAsync<LoginUserRequestMock, LoginResponseMock>(Constants.LoginApi, user)
                .GetAwaiter().GetResult();

        AdminClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", adminLoginResult?.AccessToken);

        UserClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", userLoginResult?.AccessToken);

        _checkpoint = new Checkpoint
        {
            // SchemasToInclude = new[] {"public"},
            DbAdapter = DbAdapter.Postgres, TablesToIgnore = new List<Table> {new("__EFMigrationsHistory"),}.ToArray()
        };
        _mongoRunner = MongoDbRunner.Start();
        var mongoOptions = ModuleFixture.ServiceProvider.GetService<IOptions<MongoOptions>>();
        if (mongoOptions is { })
            mongoOptions.Value.ConnectionString = _mongoRunner.ConnectionString;
    }

    public AsyncServiceScope Scope { get; }

    protected ILogger Logger { get; }

    protected HttpClient AdminClient { get; }

    protected HttpClient GuestClient { get; }

    protected HttpClient UserClient { get; }

    public ModuleFixture<TModule> ModuleFixture { get; }

    private CancellationTokenSource CancellationTokenSource { get; } = new(TimeSpan.FromSeconds(180));
    public CancellationToken CancellationToken => CancellationTokenSource.Token;

    protected virtual void RegisterTestsServices(IServiceCollection services)
    {
        var user = _integrationTestFixture.CreateAdminUserMock();
        services.ReplaceScoped(_ => user);
    }

    protected virtual void RegisterModulesTestsServices(IServiceCollection services, IModuleDefinition module)
    {
        if (module.GetType().IsAssignableTo(typeof(IModuleDefinition)))
        {
            services.Replace(new ServiceDescriptor(typeof(IHttpClientFactory),
                new DelegateHttpClientFactory(_ => GuestClient)));
        }
    }

    public async Task InitializeAsync()
    {
        CancellationToken.ThrowIfCancellationRequested();

        await ResetState();
        await SeedData();

        await ModuleFixture.ExecuteScopeAsync(async sp =>
        {
            var messagePersistenceRepository = sp.GetRequiredService<IMessagePersistenceRepository>();
            await messagePersistenceRepository.CleanupMessages();
        });

        await ModuleFixture.ServiceProvider.StartHostedServices(CancellationToken);
    }

    public async Task DisposeAsync()
    {
        await ModuleFixture.ServiceProvider.StopHostedServices(CancellationToken);
        CancellationTokenSource.Cancel();
        _mongoRunner.Dispose();
        await ModuleFixture.DisposeAsync();
        AdminClient.Dispose();
        GuestClient.Dispose();
        UserClient.Dispose();

        await Scope.DisposeAsync();
    }

    private async Task SeedData()
    {
        using (var scope = ModuleFixture.ServiceProvider.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<IDbFacadeResolver>();
            var seeders = scope.ServiceProvider.GetServices<IDataSeeder>();
            await ctx.Database.MigrateAsync(CancellationToken);

            foreach (var seeder in seeders)
            {
                await seeder.SeedAllAsync();
            }
        }
    }

    private async Task ResetState()
    {
        try
        {
            var postgresOptions = ModuleFixture.ServiceProvider.GetService<IOptions<PostgresOptions>>();
            if (postgresOptions is { } && !string.IsNullOrEmpty(postgresOptions.Value.ConnectionString))
            {
                await using var conn = new NpgsqlConnection(postgresOptions.Value.ConnectionString);
                await conn.OpenAsync(CancellationToken);

                await _checkpoint.Reset(conn);
            }
        }
        catch (Exception ex)
        {
            // ignored
        }
    }
}

public class ModuleTestBase<TEntryPoint, TModule, TWContext> : ModuleTestBase<TEntryPoint, TModule>
    where TModule : class, IModuleDefinition
    where TWContext : DbContext
    where TEntryPoint : class
{
    public ModuleTestBase(IntegrationTestFixture<TEntryPoint> integrationTestFixture, ITestOutputHelper outputHelper)
        : base(integrationTestFixture, outputHelper)
    {
        ModuleFixture = new ModuleFixture<TModule, TWContext>(
            CompositionRootRegistry.GetByModule<TModule>()!.ServiceProvider,
            integrationTestFixture.ServiceProvider.GetRequiredService<IGatewayProcessor<TModule>>());
    }

    public new ModuleFixture<TModule, TWContext> ModuleFixture { get; }
}

public class
    ModuleTestBase<TEntryPoint, TModule, TWContext, TRContext> : ModuleTestBase<TEntryPoint, TModule, TWContext>
    where TModule : class, IModuleDefinition
    where TWContext : DbContext
    where TRContext : MongoDbContext
    where TEntryPoint : class
{
    public ModuleTestBase(IntegrationTestFixture<TEntryPoint> integrationTestFixture, ITestOutputHelper outputHelper)
        : base(integrationTestFixture, outputHelper)
    {
        ModuleFixture = new ModuleFixture<TModule, TWContext, TRContext>(
            CompositionRootRegistry.GetByModule<TModule>()!.ServiceProvider,
            integrationTestFixture.ServiceProvider.GetRequiredService<IGatewayProcessor<TModule>>());
    }

    public new ModuleFixture<TModule, TWContext, TRContext> ModuleFixture { get; }
}
