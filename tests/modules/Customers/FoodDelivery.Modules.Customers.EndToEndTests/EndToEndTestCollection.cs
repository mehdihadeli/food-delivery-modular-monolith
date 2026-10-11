using FoodDelivery.Modules.Customers.Shared.Data;
using Tests.Shared.Fixtures;

namespace FoodDelivery.Modules.Customers.EndToEndTests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class EndToEndTestCollection
    : ICollectionFixture<
        SharedFixtureWithEfCoreAndMongo<global::Program, CustomersDbContext, CustomersReadDbContext>
    >
{
    public const string Name = "Customers End To End Tests";
}
