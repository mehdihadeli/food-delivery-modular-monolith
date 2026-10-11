using FoodDelivery.Modules.Customers.TestShared.Fakes.Users.Features.RegisteringUser;

namespace FoodDelivery.Modules.Customers.UnitTests.Common;

[Trait("Category", "Unit")]
public abstract class CustomerModuleUnitTestBase
{
    protected FakeUserRegistered UserRegisteredFaker { get; } = new();
}