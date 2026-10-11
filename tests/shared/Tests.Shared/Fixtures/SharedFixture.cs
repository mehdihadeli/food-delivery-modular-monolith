using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Squirrel.Abstractions.Commands;
using Squirrel.Abstractions.Messages;
using Squirrel.Abstractions.Queries;
using Squirrel.Integration.Wolverine;
using Squirrel.Persistence.EfCore.Postgres;
using Squirrel.Persistence.Mongo;
using Tests.Shared.Mocks;
using WireMock.Server;
using Wolverine;
using Wolverine.Tracking;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;
using ICommand = Squirrel.Abstractions.Commands.ICommand;
using ICommandBus = Squirrel.Abstractions.Commands.ICommandBus;
using IExternalEventBus = Squirrel.Abstractions.Messages.IExternalEventBus;
using IMessage = Squirrel.Abstractions.Messages.IMessage;
using IMessageContext = Wolverine.IMessageContext;

namespace Tests.Shared.Fixtures;

/// <summary>
/// Fully-fledged shared fixture: boots the API in-memory through <see cref="CustomWebApplicationFactory{TEntryPoint}"/>,
/// starts disposable Postgres/Mongo/Redis/RabbitMQ containers, and rewires the module configuration to point at them.
/// Share it across a collection with <c>ICollectionFixture&lt;SharedFixture&lt;Program&gt;&gt;</c>.
/// </summary>
public class SharedFixture<TEntryPoint> : IAsyncLifetime
    where TEntryPoint : class
{
    private const string EnvironmentConfigurationPrefix = "food_delivery_env_";

    /// <summary>
    /// Module names whose <c>{Module}__PostgresOptions__ConnectionString</c> /
    /// <c>{Module}__MongoOptions__ConnectionString</c> settings are redirected to the test containers.
    /// </summary>
    protected static readonly string[] DefaultModuleNames = ["Catalogs", "Customers", "Identity", "Orders"];

    private readonly IMessageSink _messageSink;
    private IHttpContextAccessor? _httpContextAccessor;
    private IServiceProvider? _serviceProvider;
    private IConfiguration? _configuration;
    private ITrackedSession? _lastTrackedSession;
    private HttpClient? _adminClient;
    private HttpClient? _normalClient;
    private HttpClient? _guestClient;

    public WireMockServer WireMockServer { get; }
    public string WireMockServerUrl { get; }
    public event Func<Task>? SharedFixtureInitialized;
    public event Func<Task>? SharedFixtureDisposed;
    public PostgresContainerFixture PostgresContainerFixture { get; }
    public RedisContainerFixture RedisContainerFixture { get; }
    public MongoContainerFixture MongoContainerFixture { get; }
    public RabbitMQContainerFixture RabbitMqContainerFixture { get; }
    public CustomWebApplicationFactory<TEntryPoint> Factory { get; }

    public IServiceProvider ServiceProvider => _serviceProvider ??= Factory.Services;
    public virtual IServiceProvider TestServiceProvider => ServiceProvider;
    public IConfiguration Configuration => _configuration ??= ServiceProvider.GetRequiredService<IConfiguration>();

    public IHttpContextAccessor HttpContextAccessor =>
        _httpContextAccessor ??= ServiceProvider.GetRequiredService<IHttpContextAccessor>();

    /// <summary>
    /// We should not dispose this GuestClient, because we reuse it in our tests.
    /// </summary>
    public HttpClient GuestClient
    {
        get
        {
            if (_guestClient == null)
            {
                _guestClient = Factory.CreateClient();
                // Set the media type of the request to JSON - we need this for getting problem details result for all http calls because problem details just return response for request with media type JSON
                _guestClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            return _guestClient;
        }
    }

    /// <summary>
    /// We should not dispose this AdminHttpClient, because we reuse it in our tests.
    /// Use <see cref="SetAdminUser"/> to authenticate the in-flight context with the admin role.
    /// </summary>
    public HttpClient AdminHttpClient => _adminClient ??= CreateAuthenticatedClient();

    /// <summary>
    /// We should not dispose this NormalUserHttpClient, because we reuse it in our tests.
    /// Use <see cref="SetNormalUser"/> to authenticate the in-flight context with the user role.
    /// </summary>
    public HttpClient NormalUserHttpClient => _normalClient ??= CreateAuthenticatedClient();

    //https://github.com/xunit/xunit/issues/565
    //https://github.com/xunit/xunit/pull/1705
    //https://xunit.net/docs/capturing-output#output-in-extensions
    //https://andrewlock.net/tracking-down-a-hanging-xunit-test-in-ci-building-a-custom-test-framework/
    public SharedFixture(IMessageSink messageSink)
    {
        _messageSink = messageSink;
        messageSink.OnMessage(new DiagnosticMessage("Constructing SharedFixture..."));

        // Service provider will build after getting with get accessors, we don't want to build our service provider here
        PostgresContainerFixture = new PostgresContainerFixture(messageSink);
        RedisContainerFixture = new RedisContainerFixture(messageSink);
        MongoContainerFixture = new MongoContainerFixture(messageSink);
        RabbitMqContainerFixture = new RabbitMQContainerFixture(messageSink);

        // new WireMockServer() is equivalent to call WireMockServer.Start()
        WireMockServer = WireMockServer.Start();
        WireMockServerUrl = WireMockServer.Url!;

        Factory = new CustomWebApplicationFactory<TEntryPoint>();
    }

    public virtual async ValueTask InitializeAsync()
    {
        _messageSink.OnMessage(new DiagnosticMessage("SharedFixture Started..."));

        await Factory.InitializeAsync();
        await PostgresContainerFixture.InitializeAsync();
        await MongoContainerFixture.InitializeAsync();
        await RabbitMqContainerFixture.InitializeAsync();
        await RedisContainerFixture.InitializeAsync();

        // Use `AddOverrideEnvKeyValues` and `__` as separator to change configs that are accessible during service
        // registration with BindOptions (module connection strings are read that way).
        Factory.AddOverrideEnvKeyValues(keyValues =>
        {
            foreach (var moduleName in DefaultModuleNames)
            {
                keyValues.Add(
                    $"{EnvironmentConfigurationPrefix}{moduleName}__{nameof(PostgresOptions)}__{nameof(PostgresOptions.ConnectionString)}",
                    PostgresContainerFixture.PostgresContainer.GetConnectionString()
                );

                keyValues.Add(
                    $"{EnvironmentConfigurationPrefix}{moduleName}__{nameof(MongoOptions)}__{nameof(MongoOptions.ConnectionString)}",
                    MongoContainerFixture.ConnectionString
                );
            }

            keyValues.Add(
                $"{EnvironmentConfigurationPrefix}{nameof(WolverineBusOptions)}__{nameof(WolverineBusOptions.RabbitMQConnectionString)}",
                RabbitMqContainerFixture.Container.GetConnectionString()
            );

            keyValues.Add(
                $"{EnvironmentConfigurationPrefix}{nameof(Squirrel.Caching.CacheOptions)}__{nameof(Squirrel.Caching.RedisDistributedCacheOptions)}__{nameof(Squirrel.Caching.RedisDistributedCacheOptions.ConnectionString)}",
                RedisContainerFixture.ConnectionString
            );
        });

        // with `AddOverrideInMemoryConfig` config changes are accessible after services registration and build process
        Factory.WithTestConfiguration(cfg =>
        {
            // Or we can override configuration explicitly, and it is accessible via IOptions<> and Configuration
            cfg["WireMockUrl"] = WireMockServerUrl;
        });

        if (SharedFixtureInitialized is not null)
        {
            await SharedFixtureInitialized.Invoke();
        }
    }

    public virtual async ValueTask DisposeAsync()
    {
        _adminClient?.Dispose();
        _normalClient?.Dispose();
        _guestClient?.Dispose();

        await Factory.DisposeAsync();

        await PostgresContainerFixture.DisposeAsync();
        await MongoContainerFixture.DisposeAsync();
        await RabbitMqContainerFixture.DisposeAsync();
        await RedisContainerFixture.DisposeAsync();

        WireMockServer.Stop();

        if (SharedFixtureDisposed is not null)
        {
            await SharedFixtureDisposed.Invoke();
        }

        _messageSink.OnMessage(new DiagnosticMessage("SharedFixture Stopped..."));
    }

    public void WithTestConfigureServices(Action<IServiceCollection> services)
    {
        Factory.WithTestConfigureServices(services);
    }

    public void WithTestConfigureAppConfiguration(Action<WebHostBuilderContext, IConfigurationBuilder> appConfiguration)
    {
        Factory.WithTestConfigureAppConfiguration(appConfiguration);
    }

    public void WithTestConfiguration(Action<IConfiguration> configurations)
    {
        Factory.WithTestConfiguration(configurations);
    }

    public void AddOverrideEnvKeyValues(Action<IDictionary<string, string>> keyValuesAction)
    {
        Factory.AddOverrideEnvKeyValues(keyValuesAction);
    }

    public void AddOverrideInMemoryConfig(Action<IDictionary<string, string>> keyValuesAction)
    {
        Factory.AddOverrideInMemoryConfig(keyValuesAction);
    }

    /// <summary>
    /// Resets the container state between tests (databases and queues).
    /// </summary>
    public async Task CleanupAsync(CancellationToken cancellationToken = default)
    {
        await PostgresContainerFixture.ResetDbAsync(cancellationToken);
        await MongoContainerFixture.ResetDbAsync(cancellationToken);
        await RabbitMqContainerFixture.CleanupQueuesAsync(cancellationToken);
    }

    public void SetAdminUser()
    {
        SetUserRole(Constants.Users.Admin.Role, Constants.Users.Admin.UserId);
    }

    public void SetNormalUser()
    {
        SetUserRole(Constants.Users.User.Role, Constants.Users.User.UserId);
    }

    public void SetUserRole(string role, string? sub = null)
    {
        sub ??= Guid.NewGuid().ToString();
        var claims = new List<Claim> { new(ClaimTypes.Role, role), new(ClaimTypes.Name, sub) };

        HttpContextAccessor.HttpContext = CreateHttpContext(claims);
    }

    public void SetUserRoles(string[] roles, string? sub = null)
    {
        sub ??= Guid.NewGuid().ToString();
        var claims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();
        claims.Add(new Claim(ClaimTypes.Name, sub));

        HttpContextAccessor.HttpContext = CreateHttpContext(claims);
    }

    public async Task ExecuteScopeAsync(Func<IServiceProvider, Task> action)
    {
        await using var scope = TestServiceProvider.CreateAsyncScope();
        await action(scope.ServiceProvider);
    }

    public async Task<T> ExecuteScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        await using var scope = TestServiceProvider.CreateAsyncScope();
        return await action(scope.ServiceProvider);
    }

    public async Task<TResponse> SendAsync<TResponse>(
        Squirrel.Abstractions.Commands.ICommand<TResponse> command,
        CancellationToken cancellationToken = default)
        where TResponse : class
    {
        TResponse? response = null;

        var trackedSession = await ExecuteScopeAsync(async sp =>
        {
            var commandBus = sp.GetRequiredService<ICommandBus>();

            return await ServiceProvider.TrackActivity()
                .ExecuteAndWaitAsync(
                    (Func<IMessageContext, Task>)(async _ =>
                        response = await commandBus.SendAsync(command, cancellationToken))
                );
        });

        RememberTrackedSession(trackedSession);

        return response!;
    }

    public async Task SendAsync(ICommand command, CancellationToken cancellationToken = default)
    {
        var trackedSession = await ExecuteScopeAsync(async sp =>
        {
            var commandBus = sp.GetRequiredService<ICommandBus>();

            return await ServiceProvider.TrackActivity()
                .ExecuteAndWaitAsync(
                    (Func<IMessageContext, Task>)(async _ => await commandBus.SendAsync(command, cancellationToken))
                );
        });

        RememberTrackedSession(trackedSession);
    }

    public async Task<TResponse> QueryAsync<TResponse>(
        Squirrel.Abstractions.Queries.IQuery<TResponse> query,
        CancellationToken cancellationToken = default)
        where TResponse : class
    {
        return await ExecuteScopeAsync(async sp =>
        {
            var queryBus = sp.GetRequiredService<IQueryBus>();

            return await queryBus.SendAsync(query, cancellationToken);
        });
    }

    public async Task PublishMessageAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var bus = scope.ServiceProvider.GetRequiredService<IExternalEventBus>();
        var trackedSession = await ServiceProvider.TrackActivity()
            .ExecuteAndWaitAsync(
                (Func<IMessageContext, Task>)(async _ => await bus.PublishAsync(message, cancellationToken))
            );

        RememberTrackedSession(trackedSession);
    }

    public Task ShouldSending<TMessage>(CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
    {
        var trackedSession = GetTrackedSession();

        trackedSession.FindEnvelopesWithMessageType<TMessage>(MessageEventType.Sent).Should().NotBeEmpty();

        return Task.CompletedTask;
    }

    public Task ShouldConsuming<TMessage>(CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
    {
        var trackedSession = GetTrackedSession();

        trackedSession.FindEnvelopesWithMessageType<TMessage>(MessageEventType.MessageSucceeded).Should().NotBeEmpty();

        return Task.CompletedTask;
    }

    // Ref: https://tech.energyhelpline.com/in-memory-testing-with-message-bus-abstractions/
    public async Task WaitUntilConditionMet(
        Func<Task<bool>> conditionToMet,
        int timeoutSecond = 60,
        string? exception = null)
    {
        var startTime = DateTime.Now;
        var timeoutExpired = false;
        var meet = await conditionToMet.Invoke();
        while (!meet)
        {
            if (timeoutExpired)
            {
                throw new TimeoutException(exception ?? "Condition not met for the test.");
            }

            await Task.Delay(100);
            meet = await conditionToMet.Invoke();
            timeoutExpired = DateTime.Now - startTime > TimeSpan.FromSeconds(timeoutSecond);
        }
    }

    private HttpClient CreateAuthenticatedClient()
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        return client;
    }

    private DefaultHttpContext CreateHttpContext(IEnumerable<Claim> claims)
    {
        var identity = new ClaimsIdentity(claims, Constants.AuthConstants.Scheme);
        var claimsPrincipal = new ClaimsPrincipal(identity);

        return new DefaultHttpContext { User = claimsPrincipal };
    }

    private ITrackedSession GetTrackedSession()
    {
        return _lastTrackedSession
            ?? throw new InvalidOperationException(
                "No Wolverine tracked session is available for the current test action."
            );
    }

    private void RememberTrackedSession(ITrackedSession trackedSession)
    {
        _lastTrackedSession = trackedSession;
    }
}
