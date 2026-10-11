using Tests.Shared.Fixtures;
using Tests.Shared.TestBase;
using FoodDelivery.Modules.Identity.Shared.Data;

namespace FoodDelivery.Modules.Identity.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
[Trait("Category", "Integration")]
public abstract class IdentityModuleIntegrationTestBase : IntegrationTestBase<global::Program, IdentityContext>
{
    protected IdentityModuleIntegrationTestBase(
        SharedFixtureWithEfCore<global::Program, IdentityContext> sharedFixture) : base(sharedFixture)
    {
    }
}
