using BuildingBlocks.Abstractions.CQRS.Command;

namespace ECommerce.Modules.Catalogs.Products.Features.ChangingProductCategory;

internal record ChangeProductCategory : ITxCommand<ChangeProductCategoryResponse>;

internal class ChangeProductCategoryHandler :
    ICommandHandler<ChangeProductCategory, ChangeProductCategoryResponse>
{
    public Task<ChangeProductCategoryResponse> Handle(
        ChangeProductCategory command,
        CancellationToken cancellationToken)
    {
        return Task.FromResult<ChangeProductCategoryResponse>(null!);
    }
}
