using BuildingBlocks.Abstractions.CQRS.Command;

namespace ECommerce.Modules.Catalogs.Products.Features.ChangingProductCategory;

internal record ChangeProductCategory : ITxCommand<ChangeProductCategoryResult>;

internal record ChangeProductCategoryResult;

internal class ChangeProductCategoryHandler :
    ICommandHandler<ChangeProductCategory, ChangeProductCategoryResult>
{
    public Task<ChangeProductCategoryResult> Handle(
        ChangeProductCategory command,
        CancellationToken cancellationToken)
    {
        return Task.FromResult<ChangeProductCategoryResult>(null!);
    }
}
