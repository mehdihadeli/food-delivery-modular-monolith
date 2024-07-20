using BuildingBlocks.Core.Domain.Exceptions;

namespace FoodDelivery.Modules.Catalogs.Products.Exceptions.Domain;

public class ProductDomainException : DomainException
{
    public ProductDomainException(string message) : base(message)
    {
    }
}
