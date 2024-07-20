using BuildingBlocks.Persistence.EfCore.Postgres;

namespace FoodDelivery.Modules.Catalogs.Shared.Data;

public class CatalogDbContextDesignFactory : DbContextDesignFactoryBase<CatalogDbContext>
{
    public CatalogDbContextDesignFactory() : base("Catalogs", "Catalogs:PostgresOptions")
    {
    }
}
