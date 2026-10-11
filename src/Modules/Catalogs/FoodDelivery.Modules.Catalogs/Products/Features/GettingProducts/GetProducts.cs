using AutoMapper;
using AutoMapper.QueryableExtensions;
using Squirrel.Core.Extensions;
using Squirrel.Core.Paging;
using Squirrel.Validation.Extensions;
using Sieve.Services;
using Squirrel.Abstractions.Queries;
using Squirrel.Core.Queries;
using Squirrel.Core.Persistence.EfCore;
using Squirrel.Core.Types;
using FoodDelivery.Modules.Catalogs.Products.Dtos;
using FoodDelivery.Modules.Catalogs.Products.Models;
using FoodDelivery.Modules.Catalogs.Shared.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Modules.Catalogs.Products.Features.GettingProducts;

public record GetProducts : PageQuery<GetProductsResponse>
{
    public static GetProducts Of(PageRequest request)
    {
        var query = new GetProducts
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SortOrder = request.SortOrder,
            Filters = request.Filters,
        };
        return new GetProductsValidator().HandleValidation(query);
    }
}

public class GetProductsValidator : AbstractValidator<GetProducts>
{
    public GetProductsValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page should at least greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("PageSize should at least greater than or equal to 1.");
    }
}

public class GetProductsHandler : IQueryHandler<GetProducts, GetProductsResponse>
{
    private readonly ICatalogDbContext _catalogDbContext;
    private readonly IMapper _mapper;
    private readonly ISieveProcessor _sieveProcessor;

    public GetProductsHandler(ICatalogDbContext catalogDbContext, IMapper mapper, ISieveProcessor sieveProcessor)
    {
        _catalogDbContext = catalogDbContext;
        _mapper = mapper;
        _sieveProcessor = sieveProcessor;
    }

    public async ValueTask<GetProductsResponse> Handle(GetProducts request, CancellationToken cancellationToken)
    {
        var products = await _catalogDbContext.Products
            .AsNoTracking()
            .ApplyPagingAsync<Product, ProductDto, DateTime?>(
                request,
                _sieveProcessor,
                query => query.ProjectTo<ProductDto>(_mapper.ConfigurationProvider),
                product => product.Created,
                cancellationToken: cancellationToken);

        return new GetProductsResponse(products);
    }
}
