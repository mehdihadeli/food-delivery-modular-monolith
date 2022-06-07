using ECommerce.Api;
using ECommerce.Modules.Customers.Shared.Data;
using Tests.Shared.Fixtures;
using Xunit.Abstractions;

namespace ECommerce.Modules.Customers.IntegrationTests;

public class CustomerModuleIntegrationTestBase : ModuleBase<Program, CustomersModuleConfiguration, CustomersDbContext, CustomersReadDbContext>
{
    public CustomerModuleIntegrationTestBase(IntegrationTestFixture<Program> integrationTestFixture,
        ITestOutputHelper outputHelper) : base(integrationTestFixture, outputHelper)
    {
    }
}
