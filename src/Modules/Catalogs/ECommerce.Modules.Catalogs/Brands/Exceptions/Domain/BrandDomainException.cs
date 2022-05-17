using BuildingBlocks.Core.Domain.Exceptions;

namespace ECommerce.Modules.Catalogs.Brands.Exceptions.Domain;

public class BrandDomainException : DomainException
{
    public BrandDomainException(string message) : base(message)
    {
    }
}
