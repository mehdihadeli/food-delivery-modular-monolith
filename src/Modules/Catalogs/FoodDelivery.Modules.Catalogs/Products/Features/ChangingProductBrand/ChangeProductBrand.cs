using Squirrel.Abstractions.Commands;

namespace FoodDelivery.Modules.Catalogs.Products.Features.ChangingProductBrand;

public record ChangeProductBrand : ITxCommand<ChangeProductBrandResponse>;

internal class ChangeProductBrandHandler :
    ICommandHandler<ChangeProductBrand, ChangeProductBrandResponse>
{
    public ValueTask<ChangeProductBrandResponse> Handle(
        ChangeProductBrand command,
        CancellationToken cancellationToken)
    {
    return ValueTask.FromResult<ChangeProductBrandResponse>(null!);
    }
}
