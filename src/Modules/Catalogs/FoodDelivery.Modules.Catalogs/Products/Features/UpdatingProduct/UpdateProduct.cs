using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Command;
using BuildingBlocks.Core.Exception;
using FoodDelivery.Modules.Catalogs.Brands.Exceptions.Application;
using FoodDelivery.Modules.Catalogs.Categories.Exceptions.Application;
using FoodDelivery.Modules.Catalogs.Products.Exceptions.Application;
using FoodDelivery.Modules.Catalogs.Products.Models;
using FoodDelivery.Modules.Catalogs.Products.ValueObjects;
using FoodDelivery.Modules.Catalogs.Shared.Contracts;
using FoodDelivery.Modules.Catalogs.Shared.Extensions;
using FoodDelivery.Modules.Catalogs.Suppliers.Exceptions.Application;
using FluentValidation;

namespace FoodDelivery.Modules.Catalogs.Products.Features.UpdatingProduct;

public record UpdateProduct(
    long Id,
    string Name,
    decimal Price,
    int RestockThreshold,
    int MaxStockThreshold,
    ProductStatus Status,
    int Width,
    int Height,
    int Depth,
    string Size,
    long CategoryId,
    long SupplierId,
    long BrandId,
    string? Description = null) : ITxUpdateCommand;

internal class UpdateProductValidator : AbstractValidator<UpdateProduct>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}

internal class UpdateProductCommandHandler : ICommandHandler<UpdateProduct>
{
    private readonly ICatalogDbContext _catalogDbContext;

    public UpdateProductCommandHandler(ICatalogDbContext catalogDbContext)
    {
        _catalogDbContext = catalogDbContext;
    }

    public async Task<Unit> Handle(UpdateProduct command, CancellationToken cancellationToken)
    {
        Guard.Against.Null(command, nameof(command));

        var product = await _catalogDbContext.FindProductByIdAsync(command.Id);
        Guard.Against.NotFound(product, new ProductNotFoundException(command.Id));

        var category = await _catalogDbContext.FindCategoryAsync(command.CategoryId);
        Guard.Against.NotFound(category, new CategoryNotFoundException(command.CategoryId));

        var brand = await _catalogDbContext.FindBrandAsync(command.BrandId);
        Guard.Against.NotFound(brand, new BrandNotFoundException(command.BrandId));

        var supplier = await _catalogDbContext.FindSupplierByIdAsync(command.SupplierId);
        Guard.Against.NotFound(supplier, new SupplierNotFoundException(command.SupplierId));

        product!.ChangeCategory(command.CategoryId);
        product.ChangeBrand(command.BrandId);
        product.ChangeSupplier(command.SupplierId);

        product.ChangeDescription(command.Description);
        product.ChangeName(command.Name);
        product.ChangePrice(command.Price);
        product.ChangeSize(command.Size);
        product.ChangeStatus(command.Status);
        product.ChangeDimensions(Dimensions.Create(command.Width, command.Height, command.Depth));
        product.ChangeMaxStockThreshold(command.MaxStockThreshold);
        product.ChangeRestockThreshold(command.RestockThreshold);

        await _catalogDbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
