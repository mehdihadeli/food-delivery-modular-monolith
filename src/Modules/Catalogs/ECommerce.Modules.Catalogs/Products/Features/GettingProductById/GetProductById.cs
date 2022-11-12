using Ardalis.GuardClauses;
using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Query;
using BuildingBlocks.Core.Exception;
using ECommerce.Modules.Catalogs.Products.Dtos;
using ECommerce.Modules.Catalogs.Products.Exceptions.Application;
using ECommerce.Modules.Catalogs.Shared.Contracts;
using ECommerce.Modules.Catalogs.Shared.Extensions;
using FluentValidation;

namespace ECommerce.Modules.Catalogs.Products.Features.GettingProductById;

public record GetProductById(long Id) : IQuery<GetProductByIdResponse>;

internal class GetProductByIdValidator : AbstractValidator<GetProductById>
{
    public GetProductByIdValidator()
    {
        CascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Id).GreaterThan(0);
    }
}

public class GetProductByIdHandler : IQueryHandler<GetProductById, GetProductByIdResponse>
{
    private readonly ICatalogDbContext _catalogDbContext;
    private readonly IMapper _mapper;

    public GetProductByIdHandler(ICatalogDbContext catalogDbContext, IMapper mapper)
    {
        _catalogDbContext = catalogDbContext;
        _mapper = mapper;
    }

    public async Task<GetProductByIdResponse> Handle(GetProductById query, CancellationToken cancellationToken)
    {
        Guard.Against.Null(query, nameof(query));

        var product = await _catalogDbContext.FindProductByIdAsync(query.Id);
        Guard.Against.NotFound(product, new ProductNotFoundException(query.Id));

        var productsDto = _mapper.Map<ProductDto>(product);

        return new GetProductByIdResponse(productsDto);
    }
}

public record GetProductByIdResponse(ProductDto Product);
