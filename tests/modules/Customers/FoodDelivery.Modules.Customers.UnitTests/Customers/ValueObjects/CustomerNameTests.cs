using FoodDelivery.Modules.Customers.Customers.Exceptions.Domain;
using FoodDelivery.Modules.Customers.Customers.Models;

namespace FoodDelivery.Modules.Customers.UnitTests.Customers.ValueObjects;

public class CustomerNameTests
{
    [Fact]
    public void create_with_valid_names_populates_all_name_fields()
    {
        var customerName = CustomerName.Create("Ada", "Lovelace");

        customerName.FirstName.Should().Be("Ada");
        customerName.LastName.Should().Be("Lovelace");
        customerName.FullName.Should().Be("Ada Lovelace");
    }

    [Theory]
    [InlineData(3)]
    [InlineData(100)]
    public void create_with_name_at_length_boundary_preserves_name(int length)
    {
        var name = new string('a', length);

        var customerName = CustomerName.Create(name, name);

        customerName.FirstName.Should().Be(name);
        customerName.LastName.Should().Be(name);
        customerName.FullName.Should().Be($"{name} {name}");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("ab")]
    public void create_with_invalid_first_name_throws(string? firstName)
    {
        var act = () => CustomerName.Create(firstName!, "Valid");

        act.Should().Throw<InvalidNameException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("ab")]
    public void create_with_invalid_last_name_throws(string? lastName)
    {
        var act = () => CustomerName.Create("Valid", lastName!);

        act.Should().Throw<InvalidNameException>();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void create_with_name_longer_than_limit_throws(bool invalidFirstName)
    {
        var tooLong = new string('a', 101);
        var firstName = invalidFirstName ? tooLong : "Valid";
        var lastName = invalidFirstName ? "Valid" : tooLong;

        var act = () => CustomerName.Create(firstName, lastName);

        act.Should().Throw<InvalidNameException>()
            .Which.Name.Should().Be(tooLong);
    }
}
