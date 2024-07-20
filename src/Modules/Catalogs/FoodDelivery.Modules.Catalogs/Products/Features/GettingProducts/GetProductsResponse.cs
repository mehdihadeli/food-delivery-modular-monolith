using BuildingBlocks.Core.CQRS.Query;
using FoodDelivery.Modules.Catalogs.Products.Dtos;

namespace FoodDelivery.Modules.Catalogs.Products.Features.GettingProducts;

public record GetProductsResponse(ListResultModel<ProductDto> Products);
