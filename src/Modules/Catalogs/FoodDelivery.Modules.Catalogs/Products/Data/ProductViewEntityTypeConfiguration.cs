using FoodDelivery.Modules.Catalogs.Products.Models;
using FoodDelivery.Modules.Catalogs.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodDelivery.Modules.Catalogs.Products.Data;

public class ProductViewEntityTypeConfiguration : IEntityTypeConfiguration<ProductView>
{
    public void Configure(EntityTypeBuilder<ProductView> builder)
    {
        builder.ToTable("product_views", CatalogDbContext.DefaultSchema);
        builder.HasKey(x => x.ProductId);
        builder.HasIndex(x => x.ProductId).IsUnique();
    }
}
