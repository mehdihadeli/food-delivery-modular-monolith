using BuildingBlocks.Core.CQRS.Query;
using ECommerce.Modules.Catalogs.Products.Dtos;

namespace ECommerce.Modules.Catalogs.Products.Features.GettingProducts;

public record GetProductsResponse(ListResultModel<ProductDto> Products);
