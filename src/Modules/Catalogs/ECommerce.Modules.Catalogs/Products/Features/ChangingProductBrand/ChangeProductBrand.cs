using BuildingBlocks.Abstractions.CQRS.Command;

namespace ECommerce.Modules.Catalogs.Products.Features.ChangingProductBrand;

internal record ChangeProductBrand : ITxCommand<ChangeProductBrandResponse>;

internal class ChangeProductBrandHandler :
    ICommandHandler<ChangeProductBrand, ChangeProductBrandResponse>
{
    public Task<ChangeProductBrandResponse> Handle(
        ChangeProductBrand command,
        CancellationToken cancellationToken)
    {
       return Task.FromResult<ChangeProductBrandResponse>(null!);
    }
}
