using Ardalis.GuardClauses;
using ECommerce.Modules.Catalogs.Brands.Exceptions.Application;

namespace ECommerce.Modules.Catalogs.Brands;

public static class GuardExtensions
{
    public static void ExistsBrand(this IGuardClause guardClause, bool exists, long brandId)
    {
        if (!exists)
            throw new BrandNotFoundException(brandId);
    }
}
