using BuildingBlocks.Abstractions.Persistence.EfCore;
using FoodDelivery.Modules.Orders.Orders.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Modules.Orders.Shared.Contracts;

public interface IOrdersDbContext : IDbContext
{
    public DbSet<Order> Orders { get; }
}
