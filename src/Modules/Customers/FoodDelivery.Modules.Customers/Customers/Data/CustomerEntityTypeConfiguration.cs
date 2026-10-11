using Squirrel.Core.Domain.ValueObjects;
using Squirrel.Core.Persistence.EfCore;
using FoodDelivery.Modules.Customers.Customers.Models;
using FoodDelivery.Modules.Customers.Customers.ValueObjects;
using FoodDelivery.Modules.Customers.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodDelivery.Modules.Customers.Customers.Data;

public class CustomerEntityTypeConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers", CustomersDbContext.DefaultSchema);

        builder.HasKey(c => c.Id);
        builder.HasIndex(x => x.Id).IsUnique();

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, id => new CustomerId(id))
            .ValueGeneratedNever();

        builder.HasIndex(x => x.IdentityId).IsUnique();

        builder.HasIndex(x => x.Email).IsUnique();
        builder.Property(x => x.Email).IsRequired()
            .HasMaxLength(EfConstants.Lenght.Medium)
            .HasConversion(email => email.Value, email => Email.Of(email)); // converting mandatory value objects

        builder.HasIndex(x => x.PhoneNumber).IsUnique();
        builder.Property(x => x.PhoneNumber)
            .IsRequired(false)
            .HasMaxLength(EfConstants.Lenght.Tiny)
            .HasConversion(x => x == null ? null : x.Value, x => x == null ? null : PhoneNumber.Of(x));

        builder.OwnsOne(m => m.Name, a =>
        {
            a.Property(p => p.FirstName)
                .HasMaxLength(EfConstants.Lenght.Medium);

            a.Property(p => p.LastName)
                .HasMaxLength(EfConstants.Lenght.Medium);

            a.Ignore(p => p.FullName);
        });

        builder.OwnsOne(m => m.Address, a =>
        {
            a.Property(p => p.City)
                .HasMaxLength(EfConstants.Lenght.Short);

            a.Property(p => p.Country)
                .HasMaxLength(EfConstants.Lenght.Medium);

            a.Property(p => p.Detail)
                .HasMaxLength(EfConstants.Lenght.Medium);

            a.Property(p => p.PostalCode)
                .IsRequired(false)
                .HasMaxLength(EfConstants.Lenght.Short)
                .HasConversion(postalCode => postalCode.Value, value => PostalCode.Of(value));
        });

        builder.Property(x => x.Nationality)
            .IsRequired(false)
            .HasMaxLength(EfConstants.Lenght.Short)
            .HasConversion(
                nationality => (string?)nationality,
                nationality => (Nationality?)nationality); // converting optional value objects

        builder.Property(x => x.BirthDate)
            .IsRequired(false)
            .HasConversion(x => x == null ? (DateTime?)null : x.Value, x => x == null ? null : BirthDate.Of(x.Value));

        builder.Property(x => x.Created).HasDefaultValueSql(EfConstants.DateAlgorithm);
    }
}
