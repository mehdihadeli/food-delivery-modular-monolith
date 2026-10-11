using FoodDelivery.Modules.Identity.TestShared.Fakes.Users.Features.RegisteringUser;

namespace FoodDelivery.Modules.Identity.UnitTests.Common;

[Trait("Category", "Unit")]
public abstract class IdentityModuleUnitTestBase
{
    protected FakeRegisterUser RegisterUserFaker { get; } = new();
}