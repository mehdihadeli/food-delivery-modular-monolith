using BuildingBlocks.Core.Exception.Types;

namespace FoodDelivery.Modules.Catalogs.Suppliers.Exceptions.Application;

public class SupplierNotFoundException : NotFoundException
{
    public SupplierNotFoundException(long id) : base($"Supplier with id '{id}' not found")
    {
    }

    public SupplierNotFoundException(string message) : base(message)
    {
    }
}
