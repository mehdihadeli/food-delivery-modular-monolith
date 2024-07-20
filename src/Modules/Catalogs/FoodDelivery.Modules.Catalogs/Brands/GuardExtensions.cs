using Ardalis.GuardClauses;
using FoodDelivery.Modules.Catalogs.Brands.Exceptions.Application;

namespace FoodDelivery.Modules.Catalogs.Brands;

public static class GuardExtensions
{
    public static void ExistsBrand(this IGuardClause guardClause, bool exists, long brandId)
    {
        if (!exists)
            throw new BrandNotFoundException(brandId);
    }
}
