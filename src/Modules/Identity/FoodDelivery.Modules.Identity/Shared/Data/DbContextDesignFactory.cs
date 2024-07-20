using BuildingBlocks.Persistence.EfCore.Postgres;

namespace FoodDelivery.Modules.Identity.Shared.Data;

public class DbContextDesignFactory : DbContextDesignFactoryBase<IdentityContext>
{
    public DbContextDesignFactory() : base("Identity", "Identity:PostgresOptions")
    {
    }
}
