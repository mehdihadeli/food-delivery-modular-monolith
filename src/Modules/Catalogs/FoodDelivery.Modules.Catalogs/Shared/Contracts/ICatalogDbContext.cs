using FoodDelivery.Modules.Catalogs.Brands;
using FoodDelivery.Modules.Catalogs.Categories;
using FoodDelivery.Modules.Catalogs.Products.Models;
using FoodDelivery.Modules.Catalogs.Suppliers;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Modules.Catalogs.Shared.Contracts;

public interface ICatalogDbContext
{
    DbSet<Product> Products { get; }
    DbSet<Category> Categories { get; }
    DbSet<Brand> Brands { get; }
    DbSet<Supplier> Suppliers { get; }
    DbSet<ProductView> ProductsView { get; }

    DbSet<TEntity> Set<TEntity>()
        where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
