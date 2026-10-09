using FoodDelivery.Modules.Customers.Customers.Exceptions.Domain;
using FoodDelivery.Modules.Customers.Customers.ValueObjects;

namespace FoodDelivery.Modules.Customers.UnitTests.Customers.ValueObjects;

public class NationalityTests
{
    [Theory]
    [InlineData("IR")]
    [InlineData("DE")]
    [InlineData("FR")]
    [InlineData("ES")]
    [InlineData("GB")]
    [InlineData("US")]
    public void create_with_supported_code_preserves_uppercase_code(string code)
    {
        var nationality = Nationality.Create(code);

        nationality.Value.Should().Be(code);
    }

    [Fact]
    public void create_with_lowercase_supported_code_normalizes_to_uppercase()
    {
        var nationality = Nationality.Create("gb");

        nationality.Value.Should().Be("GB");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("USA")]
    public void create_with_invalid_code_shape_throws(string? code)
    {
        var act = () => Nationality.Create(code!);

        act.Should().Throw<InvalidNationalityException>();
    }

    [Fact]
    public void create_with_unsupported_two_letter_code_throws()
    {
        var act = () => Nationality.Create("CA");

        act.Should().Throw<UnsupportedNationalityException>();
    }

    [Fact]
    public void implicit_conversions_preserve_supported_code()
    {
        Nationality nationality = "fr";

        string? value = nationality;

        nationality.Value.Should().Be("FR");
        value.Should().Be("FR");
    }

    [Fact]
    public void implicit_conversions_preserve_null()
    {
        Nationality? nationality = (string?)null;

        string? value = nationality;

        nationality.Should().BeNull();
        value.Should().BeNull();
    }
}
