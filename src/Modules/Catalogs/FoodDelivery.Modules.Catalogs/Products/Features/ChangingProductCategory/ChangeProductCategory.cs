using Squirrel.Abstractions.Commands;

namespace FoodDelivery.Modules.Catalogs.Products.Features.ChangingProductCategory;

public record ChangeProductCategory : ITxCommand<ChangeProductCategoryResponse>;

internal class ChangeProductCategoryHandler :
    ICommandHandler<ChangeProductCategory, ChangeProductCategoryResponse>
{
    public ValueTask<ChangeProductCategoryResponse> Handle(
        ChangeProductCategory command,
        CancellationToken cancellationToken)
    {
        return ValueTask.FromResult<ChangeProductCategoryResponse>(null!);
    }
}
