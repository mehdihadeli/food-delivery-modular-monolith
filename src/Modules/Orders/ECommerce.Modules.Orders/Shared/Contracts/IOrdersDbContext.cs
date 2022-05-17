using BuildingBlocks.Abstractions.Persistence.EfCore;
using ECommerce.Modules.Orders.Orders.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Orders.Shared.Contracts;

public interface IOrdersDbContext : IDbContext
{
    public DbSet<Order> Orders { get; }
}
