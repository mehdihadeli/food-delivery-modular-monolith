using FoodDelivery.Modules.Identity.TestShared.Fakes.Users.Features.RegisteringUser;
using Tests.Shared.Fixtures;
using Tests.Shared.TestBase;
using FoodDelivery.Modules.Identity.Shared.Data;

namespace FoodDelivery.Modules.Identity.EndToEndTests;

[Collection(EndToEndTestCollection.Name)]
[Trait("Category", "EndToEnd")]
public abstract class IdentityModuleEndToEndTestBase : EndToEndTestBase<global::Program, IdentityContext>
{
    protected IdentityModuleEndToEndTestBase(
        SharedFixtureWithEfCore<global::Program, IdentityContext> sharedFixture) : base(sharedFixture)
    {
    }

    protected FakeRegisterUserRequest RegisterUserRequestFaker { get; } = new();
}
