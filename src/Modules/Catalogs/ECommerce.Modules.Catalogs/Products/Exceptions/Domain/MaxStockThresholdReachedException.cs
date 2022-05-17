using BuildingBlocks.Core.Domain.Exceptions;

namespace ECommerce.Modules.Catalogs.Products.Exceptions.Domain;

public class MaxStockThresholdReachedException : DomainException
{
    public MaxStockThresholdReachedException(string message) : base(message)
    {
    }
}
