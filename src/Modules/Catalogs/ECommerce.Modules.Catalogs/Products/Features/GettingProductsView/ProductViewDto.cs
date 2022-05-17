namespace ECommerce.Modules.Catalogs.Products.Features.GettingProductsView;

public record struct ProductViewDto(long Id, string Name, string CategoryName, string SupplierName, long ItemCount);
