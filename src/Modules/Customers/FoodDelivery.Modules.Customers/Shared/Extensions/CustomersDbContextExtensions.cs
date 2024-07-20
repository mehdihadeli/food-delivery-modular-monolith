using FoodDelivery.Modules.Customers.Customers.Models;
using FoodDelivery.Modules.Customers.Customers.ValueObjects;
using FoodDelivery.Modules.Customers.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Modules.Customers.Shared.Extensions;

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
