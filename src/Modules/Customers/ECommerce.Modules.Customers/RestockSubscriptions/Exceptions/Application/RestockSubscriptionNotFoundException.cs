using BuildingBlocks.Core.Exception.Types;

namespace ECommerce.Modules.Customers.RestockSubscriptions.Exceptions.Application;

public class RestockSubscriptionNotFoundException : NotFoundException
{
    public RestockSubscriptionNotFoundException(long id) : base("RestockSubscription with id: " + id + " not found")
    {
    }
}
