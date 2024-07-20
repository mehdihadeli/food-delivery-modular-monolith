using BuildingBlocks.Persistence.EfCore.Postgres;

namespace FoodDelivery.Modules.Customers.Shared.Data;

public class CustomerDbContextDesignFactory : DbContextDesignFactoryBase<CustomersDbContext>
{
    public CustomerDbContextDesignFactory() : base("Customers", "Customers:PostgresOptions")
    {
    }
}
