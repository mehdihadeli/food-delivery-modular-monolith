using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tests.Shared.Mocks;
using Xunit.Abstractions;

namespace Tests.Shared.Fixtures;

// Ref: https://github.com/jbogard/ContosoUniversityDotNetCore-Pages/blob/master/ContosoUniversity.IntegrationTests/SliceFixture.cs
public class IntegrationTestFixture<TEntryPoint> : IAsyncLifetime
    where TEntryPoint : class
{
    private readonly CustomWebApplicationFactory<TEntryPoint> _factory;

    public IntegrationTestFixture()
    {
        // Ref: https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-6.0#basic-tests-with-the-default-webapplicationfactory
        _factory = new CustomWebApplicationFactory<TEntryPoint>();
    }

    public IServiceProvider ServiceProvider => _factory.Services;
    public IConfiguration Configuration => _factory.Configuration;

    // We could use `WithWebHostBuilder` method for specific config on own factory host
    public CustomWebApplicationFactory<TEntryPoint> Factory => _factory;

    public IHttpContextAccessor HttpContextAccessor =>
        ServiceProvider.GetRequiredService<IHttpContextAccessor>();

    public HttpClient CreateClient() => _factory.CreateClient();

    public void SetOutputHelper(ITestOutputHelper outputHelper)
    {
        // var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
        // loggerFactory.AddXUnit(outputHelper);
        _factory.OutputHelper = outputHelper;
    }

    public MockAuthUser CreateAdminUserMock()
    {
        var roleClaim = new Claim(ClaimTypes.Role, Constants.Users.Admin.Role);
        var otherClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Constants.Users.Admin.UserId),
            new(ClaimTypes.Name, Constants.Users.Admin.UserName),
            new(ClaimTypes.Email, Constants.Users.Admin.Email)
        };

        return _ = new MockAuthUser(otherClaims.Concat(new[] {roleClaim}).ToArray());
    }

    public MockAuthUser CreateNormalUserMock()
    {
        var roleClaim = new Claim(ClaimTypes.Role, Constants.Users.User.Role);
        var otherClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Constants.Users.User.UserId),
            new(ClaimTypes.Name, Constants.Users.User.UserName),
            new(ClaimTypes.Email, Constants.Users.User.Email)
        };

        return _ = new MockAuthUser(otherClaims.Concat(new[] {roleClaim}).ToArray());
    }

    public void SetUserRole(string role, string? sub = null)
    {
        sub ??= Guid.NewGuid().ToString();
        var claims = new List<Claim> {new Claim(ClaimTypes.Role, role), new(ClaimTypes.Name, sub)};

        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = Substitute.For<HttpContext>();
        httpContext.User.Returns(_ => claimsPrincipal);

        var httpContextAccessor = ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = httpContext;
    }

    public void SetUserRoles(string[] roles, string? sub = null)
    {
        sub ??= Guid.NewGuid().ToString();
        var claims = new List<Claim>();
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        claims.Add(new Claim(ClaimTypes.Name, sub));

        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = Substitute.For<HttpContext>();
        httpContext.User.Returns(_ => claimsPrincipal);

        var httpContextAccessor = ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        httpContextAccessor.HttpContext = httpContext;
    }

    public void RegisterTestServices(Action<IServiceCollection> services) =>
        _factory.TestRegistrationServices = services;

    public virtual async Task InitializeAsync()
    {
    }

    public virtual Task DisposeAsync()
    {
        _factory.Dispose();
        return Task.CompletedTask;
    }
}
