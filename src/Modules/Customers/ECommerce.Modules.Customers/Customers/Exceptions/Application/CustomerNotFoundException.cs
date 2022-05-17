using BuildingBlocks.Core.Exception.Types;

namespace ECommerce.Modules.Customers.Customers.Exceptions.Application;

public class CustomerNotFoundException : NotFoundException
{
    public CustomerNotFoundException(string message) : base(message)
    {
    }

    public CustomerNotFoundException(long id) : base($"Customer with id '{id}' not found.")
    {
    }
}
