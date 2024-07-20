using FoodDelivery.Modules.Orders.Orders.Models;
using FoodDelivery.Modules.Orders.Orders.ValueObjects;
using FoodDelivery.Modules.Orders.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Modules.Orders.Shared.Extensions;

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
