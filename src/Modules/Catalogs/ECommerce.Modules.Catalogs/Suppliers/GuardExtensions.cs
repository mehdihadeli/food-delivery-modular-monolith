using Ardalis.GuardClauses;
using ECommerce.Modules.Catalogs.Suppliers.Exceptions.Application;

namespace ECommerce.Modules.Catalogs.Suppliers;

public static class GuardExtensions
{
    public static void ExistsSupplier(this IGuardClause guardClause, bool exists, long supplierId)
    {
        if (exists == false)
            throw new SupplierNotFoundException(supplierId);
    }
}
