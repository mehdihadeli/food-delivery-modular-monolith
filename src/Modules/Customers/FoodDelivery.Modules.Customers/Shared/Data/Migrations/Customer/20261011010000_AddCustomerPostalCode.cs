using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FoodDelivery.Modules.Customers.Shared.Data.Migrations.Customer;

[DbContext(typeof(CustomersDbContext))]
[Migration("20261011010000_AddCustomerPostalCode")]
public sealed class AddCustomerPostalCode : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "address_postal_code",
            schema: CustomersDbContext.DefaultSchema,
            table: "customers",
            type: "character varying(25)",
            maxLength: 25,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "address_postal_code",
            schema: CustomersDbContext.DefaultSchema,
            table: "customers");
    }
}
