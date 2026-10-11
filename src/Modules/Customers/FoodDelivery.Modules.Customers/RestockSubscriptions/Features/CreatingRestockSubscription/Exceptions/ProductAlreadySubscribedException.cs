using System.Net;
using Squirrel.Core.Exception;

namespace FoodDelivery.Modules.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.Exceptions;

public class ProductAlreadySubscribedException : AppException
{
    public ProductAlreadySubscribedException(long productId, string productName)
        : base($"Product with Id '{productId}' and Name '{productName}' is already subscribed", (int)HttpStatusCode.Conflict)
    {
    }
}
