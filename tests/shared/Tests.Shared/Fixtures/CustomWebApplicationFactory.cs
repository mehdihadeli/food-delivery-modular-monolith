using Squirrel.Core.Extensions.ServiceCollectionExtensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Tests.Shared.Extensions;
using Tests.Shared.Mocks;
using Xunit;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Tests.Shared.Fixtures;

public class CustomWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>, IAsyncLifetime
    where TEntryPoint : class
{
    private readonly Dictionary<string, string?> _inMemoryConfigs = [];
    private readonly List<string> _overrideEnvKeysToDispose = [];
    private readonly List<Type> _testHostedServicesTypes = [];
    private Action<IServiceCollection>? _testConfigureServices;
    private Action<IConfiguration>? _testConfiguration;
    private Action<WebHostBuilderContext, IConfigurationBuilder>? _testConfigureAppConfiguration;
    private string _environment = "test";

    public IConfiguration Configuration => Services.GetRequiredService<IConfiguration>();
    public ITestOutputHelper? OutputHelper { get; set; }
    public Action<IServiceCollection>? TestRegistrationServices { get; set; }

    public ILogger Logger => Services.GetRequiredService<ILogger<CustomWebApplicationFactory<TEntryPoint>>>();
    public void ClearOutputHelper() => OutputHelper = null;
    public void SetOutputHelper(ITestOutputHelper value) => OutputHelper = value;

    public CustomWebApplicationFactory(Action<IServiceCollection>? testRegistrationServices = null)
    {
        TestRegistrationServices = testRegistrationServices ?? (collection => { });
    }

    public CustomWebApplicationFactory<TEntryPoint> WithTestConfigureServices(Action<IServiceCollection> services)
    {
        _testConfigureServices += services;
        return this;
    }

    public CustomWebApplicationFactory<TEntryPoint> WithTestConfiguration(Action<IConfiguration> configurations)
    {
        _testConfiguration += configurations;
        return this;
    }

    public CustomWebApplicationFactory<TEntryPoint> WithTestConfigureAppConfiguration(
        Action<WebHostBuilderContext, IConfigurationBuilder> appConfigurations)
    {
        _testConfigureAppConfiguration += appConfigurations;
        return this;
    }

    public CustomWebApplicationFactory<TEntryPoint> WithEnvironment(string environment)
    {
        _environment = environment;
        return this;
    }

    public CustomWebApplicationFactory<TEntryPoint> AddTestHostedService<THostedService>()
        where THostedService : class, IHostedService
    {
        _testHostedServicesTypes.Add(typeof(THostedService));
        return this;
    }

    /// <summary>
    /// Overrides configuration through an in-memory provider, accessible after the service provider is built via
    /// <c>IOptions&lt;&gt;</c> and <c>IConfiguration</c>.
    /// </summary>
    public CustomWebApplicationFactory<TEntryPoint> AddOverrideInMemoryConfig(
        Action<IDictionary<string, string>> inmemoryConfigsAction)
    {
        var inmemoryConfigs = new Dictionary<string, string>();
        inmemoryConfigsAction.Invoke(inmemoryConfigs);

        foreach (var inmemoryConfig in inmemoryConfigs)
        {
            // Use `TryAdd` to prevent adding repetitive elements because of using IntegrationTestBase
            _inMemoryConfigs.TryAdd(inmemoryConfig.Key, inmemoryConfig.Value);
        }

        return this;
    }

    /// <summary>
    /// Overrides configuration through environment variables, so values are visible during service registration
    /// (e.g. <c>BindOptions</c>). Uses <c>__</c> as the section separator.
    /// </summary>
    public CustomWebApplicationFactory<TEntryPoint> AddOverrideEnvKeyValues(
        Action<IDictionary<string, string>> keyValuesAction)
    {
        var keyValues = new Dictionary<string, string>();
        keyValuesAction.Invoke(keyValues);

        foreach (var (key, value) in keyValues)
        {
            _overrideEnvKeysToDispose.Add(key);
            Environment.SetEnvironmentVariable(key, value);
        }

        return this;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment(_environment);
        builder.UseContentRoot(".");

        // UseSerilog on WebHostBuilder is absolute so we should use IHostBuilder
        builder.UseSerilog(
            (ctx, loggerConfiguration) =>
            {
            }
        );

        builder.UseDefaultServiceProvider(
            (env, c) =>
            {
                // Handling Captive Dependency Problem
                // https://ankitvijay.net/2020/03/17/net-core-and-di-beware-of-captive-dependency/
                // https://blog.ploeh.dk/2014/06/02/captive-dependency/
                if (env.HostingEnvironment.IsEnvironment("test") || env.HostingEnvironment.IsDevelopment())
                    c.ValidateScopes = true;
            }
        );

        // //https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/
        // //https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration-providers#json-configuration-provider
        builder.ConfigureAppConfiguration(
            (hostingContext, configurationBuilder) =>
            {
            }
        );

        return base.CreateHost(builder);
    }

    // https://andrewlock.net/converting-integration-tests-to-net-core-3/
    // https://andrewlock.net/exploring-dotnet-6-part-6-supporting-integration-tests-with-webapplicationfactory-in-dotnet-6/
    // https://github.com/dotnet/aspnetcore/pull/33462
    // https://github.com/dotnet/aspnetcore/issues/33846
    // https://milestone.topics.it/2021/04/28/you-wanna-test-http.html
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        //// add in-memory configuration instead of using appsettings.json and override existing settings, accessible via IOptions<> and IConfiguration
        //// https://blog.markvincze.com/overriding-configuration-in-asp-net-core-integration-tests/
        builder.ConfigureAppConfiguration(
            (hostingContext, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(_inMemoryConfigs);

                _testConfiguration?.Invoke(hostingContext.Configuration);
                _testConfigureAppConfiguration?.Invoke(hostingContext, configurationBuilder);
            }
        );

        //The test app's builder.ConfigureServices callback is executed before the SUT's Startup.ConfigureServices code.
        builder.ConfigureServices(services =>
        {
            services.AddScoped<TextWriter>(_ => new StringWriter());
            services.AddScoped<TextReader>(sp =>
                new StringReader(sp.GetRequiredService<TextWriter>().ToString() ?? ""));
        });

        //The test app's builder.ConfigureTestServices callback is executed after the app's Startup.ConfigureServices code is executed.
        builder.ConfigureTestServices(services =>
        {
            // services.RemoveAll(typeof(IHostedService));

            // add test-hosted services requested by the test
            foreach (var hostedServiceType in _testHostedServicesTypes)
            {
                services.AddSingleton(typeof(IHostedService), hostedServiceType);
            }

            services.AddScoped(_ => CreateAnonymouslyUserMock());

            // https://milestone.topics.it/2021/11/10/http-client-factory-in-integration-testing.html
            services.Replace(new ServiceDescriptor(typeof(IHttpClientFactory),
                new DelegateHttpClientFactory(ClientProvider)));

            // This helper just supports jwt Scheme, and for Identity server Scheme will crash so we should disable AddIdentityServer()
            services.Replace(ServiceDescriptor.Singleton<IHttpContextAccessor>(CreateHttpContextAccessorMock));
            services.AddTestAuthentication();

            _testConfigureServices?.Invoke(services);
            TestRegistrationServices?.Invoke(services);
            services.AddKeyedSingleton<Action<IServiceCollection>>("ModuleTestServices", (_, _) => moduleServices =>
            {
                moduleServices.Replace(new ServiceDescriptor(typeof(IHttpClientFactory),
                    new DelegateHttpClientFactory(ClientProvider)));
                _testConfigureServices?.Invoke(moduleServices);
                TestRegistrationServices?.Invoke(moduleServices);
            });
        });
    }

    public HttpClient ClientProvider(string name)
    {
        return CreateClient();
    }

    private static IHttpContextAccessor CreateHttpContextAccessorMock(IServiceProvider serviceProvider)
    {
        var httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
        using var scope = serviceProvider.CreateScope();
        httpContextAccessorMock.HttpContext = new DefaultHttpContext { RequestServices = scope.ServiceProvider, };

        httpContextAccessorMock.HttpContext.Request.Host = new HostString("localhost", 5000);
        httpContextAccessorMock.HttpContext.Request.Scheme = "http";
        var res = httpContextAccessorMock.HttpContext.AuthenticateAsync(Constants.AuthConstants.Scheme).GetAwaiter()
            .GetResult();
        httpContextAccessorMock.HttpContext.User = res.Ticket?.Principal!;
        return httpContextAccessorMock;
    }

    private MockAuthUser CreateAnonymouslyUserMock()
    {
        return new MockAuthUser();
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            CleanupOverrideEnvKeys();

        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        CleanupOverrideEnvKeys();

        await base.DisposeAsync();
    }

    private void CleanupOverrideEnvKeys()
    {
        foreach (var disposeEnvKey in _overrideEnvKeysToDispose)
        {
            Environment.SetEnvironmentVariable(disposeEnvKey, null);
        }

        _overrideEnvKeysToDispose.Clear();
    }
}
