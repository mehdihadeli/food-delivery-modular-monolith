using Squirrel.Core.Exception;

namespace FoodDelivery.Modules.Customers.RestockSubscriptions.Exceptions.Domain;

public class RestockSubscriptionDomainException : DomainException
{
    public RestockSubscriptionDomainException(string message) : base(message)
    {
    }
}
