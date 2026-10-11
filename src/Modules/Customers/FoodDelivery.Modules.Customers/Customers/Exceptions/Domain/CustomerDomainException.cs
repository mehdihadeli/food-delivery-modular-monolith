using System.Net;
using Squirrel.Core.Exception;

namespace FoodDelivery.Modules.Customers.Customers.Exceptions.Domain;

public class CustomerDomainException : DomainException
{
    public CustomerDomainException(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest) :
        base(message, (int)statusCode)
    {
    }
}
