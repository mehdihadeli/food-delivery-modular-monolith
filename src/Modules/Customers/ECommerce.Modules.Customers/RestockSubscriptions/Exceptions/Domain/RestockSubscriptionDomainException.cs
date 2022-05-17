using BuildingBlocks.Core.Domain.Exceptions;

namespace ECommerce.Modules.Customers.RestockSubscriptions.Exceptions.Domain;

public class RestockSubscriptionDomainException : DomainException
{
    public RestockSubscriptionDomainException(string message) : base(message)
    {
    }
}
