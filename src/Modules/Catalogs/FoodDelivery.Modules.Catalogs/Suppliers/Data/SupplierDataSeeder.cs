using Bogus;
using Squirrel.Abstractions.Persistence;
using FoodDelivery.Modules.Catalogs.Shared.Data;
using FoodDelivery.Modules.Catalogs.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Modules.Catalogs.Suppliers.Data;

public class SupplierDataSeeder : IDataSeeder<CatalogDbContext>
{
    private readonly ICatalogDbContext _dbContext;

    public SupplierDataSeeder(ICatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAsync(CatalogDbContext context)
    {
        if (await _dbContext.Suppliers.AnyAsync())
            return;

        long id = 1;
        var suppliersFaker = new Faker<Supplier>().CustomInstantiator(faker =>
        {
            var supplier = new Supplier(id, faker.Person.FullName);
            id++;
            return supplier;
        });

        var suppliers = suppliersFaker.Generate(5);
        await _dbContext.Suppliers.AddRangeAsync(suppliers);

        await _dbContext.SaveChangesAsync();
    }
}
