using BuildingBlocks.Abstractions.Persistence;

namespace FoodDelivery.Modules.Customers.Customers.Data;

public class CustomersDataSeeder : IDataSeeder
{
    public Task SeedAllAsync()
    {
        return Task.CompletedTask;
    }
}
