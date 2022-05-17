using Ardalis.GuardClauses;
using ECommerce.Modules.Catalogs.Categories.Exceptions.Application;

namespace ECommerce.Modules.Catalogs.Categories;

public static class GuardExtensions
{
    public static void ExistsCategory(this IGuardClause guardClause, bool exists, long categoryId)
    {
        if (exists == false)
            throw new CategoryNotFoundException(categoryId);
    }
}
