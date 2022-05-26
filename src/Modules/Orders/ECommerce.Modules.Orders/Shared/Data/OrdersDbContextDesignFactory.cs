using BuildingBlocks.Persistence.EfCore.Postgres;

namespace ECommerce.Modules.Orders.Shared.Data;

public class OrdersDbContextDesignFactory : DbContextDesignFactoryBase<OrdersDbContext>
{
    public OrdersDbContextDesignFactory() : base("Orders:PostgresOptions")
    {
    }
}
