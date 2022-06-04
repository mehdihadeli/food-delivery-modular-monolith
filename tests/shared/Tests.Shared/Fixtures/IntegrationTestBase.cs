using System.Net.Http.Headers;
using BuildingBlocks.Abstractions.Web;
using BuildingBlocks.Abstractions.Web.Module;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Web.Module;
using ECommerce.Modules.Catalogs;
using ECommerce.Modules.Catalogs.Shared.Data;
using ECommerce.Modules.Customers;
using ECommerce.Modules.Customers.Shared.Data;
using ECommerce.Modules.Identity;
using ECommerce.Modules.Identity.Shared.Data;
using ECommerce.Modules.Orders;
using ECommerce.Modules.Orders.Shared.Data;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Tests.Shared.Mocks;
using Tests.Shared.Mocks.Builders;

namespace Tests.Shared.Fixtures;

public abstract class IntegrationTestBase<TEntryPoint, TDbContext> : IntegrationTestBase<TEntryPoint>
    where TDbContext : DbContext
    where TEntryPoint : class
{
    protected IntegrationTestBase(IntegrationTestFixture<TEntryPoint, TDbContext> integrationTestFixture,
        ITestOutputHelper
            outputHelper)
        : base(integrationTestFixture, outputHelper)
    {
    }
}

public abstract class IntegrationTestBase<TEntryPoint> : IClassFixture<IntegrationTestFixture<TEntryPoint>>,IDisposable
    where TEntryPoint : class
{
    protected IntegrationTestBase(
        IntegrationTestFixture<TEntryPoint> integrationTestFixture,
        ITestOutputHelper outputHelper)
    {
        IntegrationTestFixture = integrationTestFixture;
        integrationTestFixture.RegisterTestServices(RegisterTestsServices);
        ModuleHook.ModuleServicesConfigured += RegisterModulesTestsServices;
        integrationTestFixture.SetOutputHelper(outputHelper);

        Scope = integrationTestFixture.ServiceProvider.CreateScope();

        CatalogsModule = new ModuleFixture<CatalogModuleConfiguration, CatalogDbContext, CatalogReadDbContext>(
            CompositionRootRegistry.GetByModule<CatalogModuleConfiguration>()!.ServiceProvider,
            Scope.ServiceProvider.GetRequiredService<IGatewayProcessor<CatalogModuleConfiguration>>(),
            CatalogModuleConfiguration.ModuleName);

        IdentityModule = new ModuleFixture<IdentityModuleConfiguration, IdentityContext>(
            CompositionRootRegistry.GetByModule<IdentityModuleConfiguration>()!.ServiceProvider,
            Scope.ServiceProvider.GetRequiredService<IGatewayProcessor<IdentityModuleConfiguration>>(),
            IdentityModuleConfiguration.ModuleName);

        CustomersModule = new ModuleFixture<CustomersModuleConfiguration, CustomersDbContext, CustomersReadDbContext>(
            CompositionRootRegistry.GetByModule<CustomersModuleConfiguration>()!.ServiceProvider,
            Scope.ServiceProvider.GetRequiredService<IGatewayProcessor<CustomersModuleConfiguration>>(),
            CustomersModuleConfiguration.ModuleName);

        OrdersModule = new ModuleFixture<OrdersModuleConfiguration, OrdersDbContext, OrderReadDbContext>(
            CompositionRootRegistry.GetByModule<OrdersModuleConfiguration>()!.ServiceProvider,
            Scope.ServiceProvider.GetRequiredService<IGatewayProcessor<OrdersModuleConfiguration>>(),
            OrdersModuleConfiguration.ModuleName);


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

    protected CancellationTokenSource CancellationTokenSource { get; } = new(TimeSpan.FromSeconds(60));
    protected IServiceScope Scope { get; }
    protected IntegrationTestFixture<TEntryPoint> IntegrationTestFixture { get; }
    protected ILogger Logger { get; }
    public CancellationToken CancellationToken => CancellationTokenSource.Token;
    protected TextWriter TextWriter => Scope.ServiceProvider.GetRequiredService<TextWriter>();
    protected HttpClient AdminClient { get; }
    protected HttpClient GuestClient { get; }
    protected HttpClient UserClient { get; }

    public ModuleFixture<CatalogModuleConfiguration, CatalogDbContext, CatalogReadDbContext> CatalogsModule { get; }

    public ModuleFixture<CustomersModuleConfiguration, CustomersDbContext, CustomersReadDbContext> CustomersModule
    {
        get;
    }

    public ModuleFixture<IdentityModuleConfiguration, IdentityContext> IdentityModule { get; }
    public ModuleFixture<OrdersModuleConfiguration, OrdersDbContext, OrderReadDbContext> OrdersModule { get; }

    protected virtual void RegisterTestsServices(IServiceCollection services)
    {
    }

    protected virtual void RegisterModulesTestsServices(IServiceCollection services, IModuleDefinition module)
    {
        if (module.GetType().IsAssignableTo(typeof(IModuleDefinition)))
        {
            services.Replace(new ServiceDescriptor(typeof(IHttpClientFactory),
                new DelegateHttpClientFactory(_ => GuestClient)));
        }
    }

    public void Dispose()
    {
        CancellationTokenSource.Dispose();
        Scope.Dispose();
        AdminClient.Dispose();
        GuestClient.Dispose();
        UserClient.Dispose();
        CustomersModule?.Dispose();
        IdentityModule?.Dispose();
        CatalogsModule?.Dispose();
        OrdersModule?.Dispose();
    }
}
