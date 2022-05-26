using BuildingBlocks.Persistence.EfCore.Postgres;

namespace ECommerce.Modules.Customers.Shared.Data;

public class CustomerDbContextDesignFactory : DbContextDesignFactoryBase<CustomersDbContext>
{
    public CustomerDbContextDesignFactory() : base("Customers:PostgresOptions")
    {
    }
}
