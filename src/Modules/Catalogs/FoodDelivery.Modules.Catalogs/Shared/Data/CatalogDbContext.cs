using BuildingBlocks.Core.Persistence.EfCore;
using FoodDelivery.Modules.Catalogs.Brands;
using FoodDelivery.Modules.Catalogs.Categories;
using FoodDelivery.Modules.Catalogs.Products.Models;
using FoodDelivery.Modules.Catalogs.Shared.Contracts;
using FoodDelivery.Modules.Catalogs.Suppliers;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Modules.Catalogs.Shared.Data;

public class CatalogDbContext : EfDbContextBase, ICatalogDbContext
{
    public const string DefaultSchema = "catalog";

    public CatalogDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductView> ProductsView => Set<ProductView>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Brand> Brands => Set<Brand>();
}
