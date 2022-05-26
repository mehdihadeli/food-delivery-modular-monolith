using BuildingBlocks.Persistence.EfCore.Postgres;

namespace ECommerce.Modules.Catalogs.Shared.Data;

public class CatalogDbContextDesignFactory : DbContextDesignFactoryBase<CatalogDbContext>
{
    public CatalogDbContextDesignFactory() : base("Catalogs:PostgresOptions")
    {
    }
}
