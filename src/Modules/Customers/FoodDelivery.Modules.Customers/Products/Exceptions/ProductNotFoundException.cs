using Squirrel.Core.Exception;

namespace FoodDelivery.Modules.Customers.Products.Exceptions;

public class ProductNotFoundException : NotFoundException
{
    public ProductNotFoundException(long id) : base($"Product with id {id} not found")
    {
    }
}
