using ECommerce.Modules.Catalogs.Brands;
using ECommerce.Modules.Catalogs.Categories;
using ECommerce.Modules.Catalogs.Products.Models;
using ECommerce.Modules.Catalogs.Suppliers;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Catalogs.Shared.Contracts;

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
