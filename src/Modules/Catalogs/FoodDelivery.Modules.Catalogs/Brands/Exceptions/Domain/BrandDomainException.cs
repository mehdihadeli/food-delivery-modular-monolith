using BuildingBlocks.Core.Domain.Exceptions;

namespace FoodDelivery.Modules.Catalogs.Brands.Exceptions.Domain;

public class BrandDomainException : DomainException
{
    public BrandDomainException(string message) : base(message)
    {
    }
}
