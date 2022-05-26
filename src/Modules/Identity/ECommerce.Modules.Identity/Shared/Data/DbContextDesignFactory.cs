using BuildingBlocks.Persistence.EfCore.Postgres;

namespace ECommerce.Modules.Identity.Shared.Data;

public class DbContextDesignFactory : DbContextDesignFactoryBase<IdentityContext>
{
    public DbContextDesignFactory() : base("Identity:PostgresOptions")
    {
    }
}
