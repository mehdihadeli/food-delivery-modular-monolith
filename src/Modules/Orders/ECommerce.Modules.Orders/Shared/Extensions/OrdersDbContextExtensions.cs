using ECommerce.Modules.Orders.Orders.Models;
using ECommerce.Modules.Orders.Orders.ValueObjects;
using ECommerce.Modules.Orders.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Orders.Shared.Extensions;

public static class OrdersDbContextExtensions
{
    public static ValueTask<Order?> FindOrderByIdAsync(this OrdersDbContext context, OrderId id)
    {
        return context.Orders.FindAsync(id);
    }

    public static Task<bool> ExistsOrderByIdAsync(this OrdersDbContext context, OrderId id)
    {
        return context.Orders.AnyAsync(x => x.Id == id);
    }
}
