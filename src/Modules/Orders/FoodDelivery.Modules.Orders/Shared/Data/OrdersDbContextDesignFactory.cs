using BuildingBlocks.Persistence.EfCore.Postgres;

namespace FoodDelivery.Modules.Orders.Shared.Data;

public class OrdersDbContextDesignFactory : DbContextDesignFactoryBase<OrdersDbContext>
{
    public OrdersDbContextDesignFactory() : base("Orders", "Orders:PostgresOptions")
    {
    }
}
