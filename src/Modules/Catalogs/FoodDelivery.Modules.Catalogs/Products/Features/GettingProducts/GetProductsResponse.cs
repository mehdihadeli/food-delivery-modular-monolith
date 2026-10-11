using Squirrel.Abstractions.Core.Paging;
using FoodDelivery.Modules.Catalogs.Products.Dtos;

namespace FoodDelivery.Modules.Catalogs.Products.Features.GettingProducts;

public record GetProductsResponse(IPageList<ProductDto> Products);
