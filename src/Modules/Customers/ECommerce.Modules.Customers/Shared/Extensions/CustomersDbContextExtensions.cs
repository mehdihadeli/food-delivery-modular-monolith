using ECommerce.Modules.Customers.Customers.Models;
using ECommerce.Modules.Customers.Customers.ValueObjects;
using ECommerce.Modules.Customers.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Customers.Shared.Extensions;

public static class CustomersDbContextExtensions
{
    public static ValueTask<Customer?> FindCustomerByIdAsync(this CustomersDbContext context, CustomerId id)
    {
        return context.Customers.FindAsync(id);
    }

    public static Task<bool> ExistsCustomerByIdAsync(this CustomersDbContext context, CustomerId id)
    {
        return context.Customers.AnyAsync(x => x.Id == id);
    }
}
