using BuildingBlocks.Core.Domain.Exceptions;

namespace FoodDelivery.Modules.Catalogs.Products.Exceptions.Domain;

public class InsufficientStockException : DomainException
{
    public InsufficientStockException(string message) : base(message)
    {
    }
}
