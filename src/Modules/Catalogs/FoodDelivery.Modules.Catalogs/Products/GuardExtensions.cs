using Ardalis.GuardClauses;
using FoodDelivery.Modules.Catalogs.Products.Exceptions.Application;

namespace FoodDelivery.Modules.Catalogs.Products;

public static class GuardExtensions
{
    public static void ExistsProduct(this IGuardClause guardClause, bool exists, long productId)
    {
        if (exists == false)
            throw new ProductNotFoundException(productId);
    }
}
