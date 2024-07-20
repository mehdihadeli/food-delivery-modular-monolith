using BuildingBlocks.Core.Exception.Types;

namespace FoodDelivery.Modules.Customers.Customers.Exceptions.Domain;

internal class CustomerAlreadyCompletedException : AppException
{
    public long CustomerId { get; }

    public CustomerAlreadyCompletedException(long customerId)
        : base($"Customer with ID: '{customerId}' already completed.")
    {
        CustomerId = customerId;
    }
}
