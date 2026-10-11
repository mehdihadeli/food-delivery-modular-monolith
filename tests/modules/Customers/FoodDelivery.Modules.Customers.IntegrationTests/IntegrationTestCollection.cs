using FoodDelivery.Modules.Customers.Shared.Data;
using Tests.Shared.Fixtures;

namespace FoodDelivery.Modules.Customers.IntegrationTests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class IntegrationTestCollection
    : ICollectionFixture<
        SharedFixtureWithEfCoreAndMongo<global::Program, CustomersDbContext, CustomersReadDbContext>
    >
{
    public const string Name = "Customers Integration Tests";
}
