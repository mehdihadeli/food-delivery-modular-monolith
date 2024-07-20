using BuildingBlocks.Core.Extensions.ServiceCollection;
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
using Xunit.Abstractions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Tests.Shared.Fixtures;

public class CustomWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>
    where TEntryPoint : class
{
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

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("test");
        builder.UseContentRoot(".");

        // UseSerilog on WebHostBuilder is absolute so we should use IHostBuilder
        builder.UseSerilog(
            (ctx, loggerConfiguration) =>
            {
                //https://github.com/trbenning/serilog-sinks-xunit
                if (OutputHelper is not null)
                {
                    loggerConfiguration.WriteTo.TestOutput(
                        OutputHelper,
                        LogEventLevel.Information,
                        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level} - {Message:lj}{NewLine}{Exception}"
                    );
                }
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

            services.AddScoped(_ => CreateAnonymouslyUserMock());

            // https://milestone.topics.it/2021/11/10/http-client-factory-in-integration-testing.html
            services.Replace(new ServiceDescriptor(typeof(IHttpClientFactory),
                new DelegateHttpClientFactory(ClientProvider)));

            // This helper just supports jwt Scheme, and for Identity server Scheme will crash so we should disable AddIdentityServer()
            services.ReplaceSingleton(CreateHttpContextAccessorMock);
            services.AddTestAuthentication();

            TestRegistrationServices?.Invoke(services);
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
}
