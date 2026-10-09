using Squirrel.Core.Exception;

namespace FoodDelivery.Modules.Catalogs.Brands.Exceptions.Application;

public class BrandNotFoundException : NotFoundException
{
    public BrandNotFoundException(long id) : base($"Brand with id '{id}' not found")
    {
    }

    public BrandNotFoundException(string message) : base(message)
    {
    }
}
