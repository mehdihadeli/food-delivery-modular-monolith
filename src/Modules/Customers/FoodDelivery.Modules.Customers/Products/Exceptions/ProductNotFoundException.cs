using BuildingBlocks.Core.Exception.Types;

namespace FoodDelivery.Modules.Customers.Products.Exceptions;

public class ProductNotFoundException : NotFoundException
{
    public ProductNotFoundException(long id) : base($"Product with id {id} not found")
    {
    }
}
