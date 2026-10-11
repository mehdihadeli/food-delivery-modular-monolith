namespace FoodDelivery.Modules.Identity.IntegrationTests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class IntegrationTestCollection : ICollectionFixture<Tests.Shared.Fixtures.SharedFixtureWithEfCore<global::Program, FoodDelivery.Modules.Identity.Shared.Data.IdentityContext>>
{
    public const string Name = "Identity Integration Tests";
}
