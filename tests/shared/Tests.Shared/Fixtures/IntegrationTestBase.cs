using System.Net.Http.Headers;
using BuildingBlocks.Abstractions.Web;
using BuildingBlocks.Abstractions.Web.Module;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Web.Module;
using ECommerce.Modules.Catalogs;
using ECommerce.Modules.Customers;
using ECommerce.Modules.Customers.Shared.Clients.Identity;
using ECommerce.Modules.Identity;
using ECommerce.Modules.Orders;
using ECommerce.Modules.Orders.Orders.Models;
using Microsoft.Extensions.Logging;
using Tests.Shared.Builders;
using Tests.Shared.Mocks;

namespace Tests.Shared.Fixtures;

public abstract class IntegrationTestBase<TEntryPoint, TDbContext> : IntegrationTestBase<TEntryPoint>
    where TEntryPoint : class
    where TDbContext : DbContext
{
    protected IntegrationTestBase(
        IntegrationTestFixture<TEntryPoint, TDbContext> integrationTestFixture,
        ITestOutputHelper outputHelper) : base(integrationTestFixture, outputHelper)
    {
    }
}

public abstract class IntegrationTestBase<TEntryPoint> : IClassFixture<IntegrationTestFixture<TEntryPoint>>
    where TEntryPoint : class
{
    protected CancellationTokenSource CancellationTokenSource { get; } = new(TimeSpan.FromSeconds(60));
    protected IServiceScope Scope { get; }
    protected IntegrationTestFixture<TEntryPoint> IntegrationTestFixture { get; }

    protected ILogger Logger { get; }
    public CancellationToken CancellationToken => CancellationTokenSource.Token;
    protected TextWriter TextWriter => Scope.ServiceProvider.GetRequiredService<TextWriter>();

    protected HttpClient AdminClient { get; }
    protected HttpClient GuestClient { get; }
    protected HttpClient UserClient { get; }

    public ModuleFixture<CatalogModuleConfiguration> CatalogModule { get; }
    public ModuleFixture<CustomersModuleConfiguration> CustomersModule { get; }
    public ModuleFixture<IdentityModuleConfiguration> IdentityModule { get; }
    public ModuleFixture<OrdersModuleConfiguration> OrderModule { get; }

    protected IntegrationTestBase(
        IntegrationTestFixture<TEntryPoint> integrationTestFixture,
        ITestOutputHelper outputHelper)
    {
        IntegrationTestFixture = integrationTestFixture;
        integrationTestFixture.RegisterTestServices(RegisterTestsServices);
        ModuleHook.ModuleServicesConfigured += RegisterModulesTestsServices;

        Scope = integrationTestFixture.ServiceProvider.CreateScope();

        CatalogModule = new ModuleFixture<CatalogModuleConfiguration>(
            CompositionRootRegistry.GetByModule<CatalogModuleConfiguration>()!.ServiceProvider,
            Scope.ServiceProvider.GetRequiredService<IGatewayProcessor<CatalogModuleConfiguration>>(),
            "CatalogModule");

        IdentityModule = new ModuleFixture<IdentityModuleConfiguration>(
            CompositionRootRegistry.GetByModule<IdentityModuleConfiguration>()!.ServiceProvider,
            Scope.ServiceProvider.GetRequiredService<IGatewayProcessor<IdentityModuleConfiguration>>(),
            "IdentityModule");

        CustomersModule = new ModuleFixture<CustomersModuleConfiguration>(
            CompositionRootRegistry.GetByModule<CustomersModuleConfiguration>()!.ServiceProvider,
            Scope.ServiceProvider.GetRequiredService<IGatewayProcessor<CustomersModuleConfiguration>>(),
            "CustomersModule");

        OrderModule = new ModuleFixture<OrdersModuleConfiguration>(
            CompositionRootRegistry.GetByModule<OrdersModuleConfiguration>()!.ServiceProvider,
            Scope.ServiceProvider.GetRequiredService<IGatewayProcessor<OrdersModuleConfiguration>>(),
            "OrderModule");


        integrationTestFixture.SetOutputHelper(outputHelper);
        Logger = Scope.ServiceProvider.GetRequiredService<ILogger<IntegrationTestFixture<TEntryPoint>>>();

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
    }

    protected virtual void RegisterTestsServices(IServiceCollection services)
    {
    }

    protected virtual void RegisterModulesTestsServices(IServiceCollection services, IModuleDefinition module)
    {
    }
}
