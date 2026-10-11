using FoodDelivery.Modules.Customers.Shared.Data;
using FoodDelivery.Modules.Customers.TestShared.Fakes.Users.Features.RegisteringUser;
using Tests.Shared.Fixtures;
using Tests.Shared.TestBase;

namespace FoodDelivery.Modules.Customers.EndToEndTests;

[Collection(EndToEndTestCollection.Name)]
[Trait("Category", "EndToEnd")]
public abstract class CustomerModuleEndToEndTestBase(
    SharedFixtureWithEfCoreAndMongo<global::Program, CustomersDbContext, CustomersReadDbContext> sharedFixture
) : EndToEndTestBase<global::Program, CustomersDbContext, CustomersReadDbContext>(sharedFixture)
{
    protected FakeUserRegistered UserRegisteredFaker { get; } = new();
}
