using System.Reflection;
using BuildingBlocks.Core.Persistence.EfCore;
using ECommerce.Modules.Orders.Orders.Models;
using ECommerce.Modules.Orders.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Orders.Shared.Data;

public class OrdersDbContext : EfDbContextBase, IOrdersDbContext
{
    public const string DefaultSchema = "order";

    public OrdersDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension(EfConstants.UuidGenerator);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public DbSet<Order> Orders => Set<Order>();
}
