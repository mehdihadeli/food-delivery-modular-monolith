using Squirrel.Abstractions.Commands;

namespace FoodDelivery.Modules.Catalogs.Products.Features.ChangingProductSupplier;

public record ChangeProductSupplier : ITxCommand<ChangeProductSupplierResponse>;

internal class ChangeProductSupplierCommandHandler :
    ICommandHandler<ChangeProductSupplier, ChangeProductSupplierResponse>
{
    public ValueTask<ChangeProductSupplierResponse> Handle(
        ChangeProductSupplier request,
        CancellationToken cancellationToken)
    {
        return ValueTask.FromResult<ChangeProductSupplierResponse>(null!);
    }
}
