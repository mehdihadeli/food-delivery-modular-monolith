namespace FoodDelivery.Modules.Identity.EndToEndTests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class EndToEndTestCollection : ICollectionFixture<Tests.Shared.Fixtures.SharedFixtureWithEfCore<global::Program, FoodDelivery.Modules.Identity.Shared.Data.IdentityContext>>
{
    public const string Name = "Identity End To End Tests";
}
