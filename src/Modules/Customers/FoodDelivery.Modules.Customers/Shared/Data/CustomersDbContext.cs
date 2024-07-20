using BuildingBlocks.Core.Persistence.EfCore;
using FoodDelivery.Modules.Customers.Customers.Models;
using FoodDelivery.Modules.Customers.RestockSubscriptions.Models.Write;
using FoodDelivery.Modules.Customers.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Modules.Customers.Shared.Data;

public class CustomersDbContext : EfDbContextBase, ICustomersDbContext
{
    public const string DefaultSchema = "customer";

    public CustomersDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension(EfConstants.UuidGenerator);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<RestockSubscription> RestockSubscriptions => Set<RestockSubscription>();

    public override void Dispose()
    {
        base.Dispose();
    }
}
