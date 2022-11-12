using BuildingBlocks.Abstractions.CQRS.Command;

namespace ECommerce.Modules.Catalogs.Products.Features.ChangingProductSupplier;

internal record ChangeProductSupplier : ITxCommand<ChangeProductSupplierResponse>;

internal class ChangeProductSupplierCommandHandler :
    ICommandHandler<ChangeProductSupplier, ChangeProductSupplierResponse>
{
    public Task<ChangeProductSupplierResponse> Handle(
        ChangeProductSupplier request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult<ChangeProductSupplierResponse>(null!);
    }
}
