using Squirrel.Abstractions.Persistence;
using FoodDelivery.Modules.Customers.Shared.Data;

namespace FoodDelivery.Modules.Customers.Customers.Data;

public class CustomersDataSeeder : IDataSeeder<CustomersDbContext>
{
    public Task SeedAsync(CustomersDbContext context)
    {
        return Task.CompletedTask;
    }
}
