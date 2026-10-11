using FoodDelivery.Modules.Customers.Shared.Data;
using Tests.Shared.Fixtures;
using Tests.Shared.TestBase;
using Xunit;

namespace FoodDelivery.Modules.Customers.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
[Trait("Category", "Integration")]
public abstract class CustomerModuleTestIntegrationTestBase(
    SharedFixtureWithEfCoreAndMongo<global::Program, CustomersDbContext, CustomersReadDbContext> sharedFixture
) : IntegrationTestBase<global::Program, CustomersDbContext, CustomersReadDbContext>(sharedFixture)
{
}
