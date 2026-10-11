using FoodDelivery.Modules.Customers.Customers.ValueObjects;
using FoodDelivery.Modules.Customers.UnitTests.Common;

namespace FoodDelivery.Modules.Customers.UnitTests.Customers.ValueObjects;

public class CustomerIdTests : CustomerModuleUnitTestBase
{
    [Fact]
    public void constructor_with_positive_value_preserves_value()
    {
        var customerId = new CustomerId(42);

        customerId.Value.Should().Be(42);
    }

    [Fact]
    public void implicit_conversion_from_long_creates_customer_id()
    {
        CustomerId customerId = 73L;

        customerId.Value.Should().Be(73);
    }

    [Fact]
    public void implicit_conversion_to_long_returns_value()
    {
        var customerId = new CustomerId(91);

        long value = customerId;

        value.Should().Be(91);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void constructor_with_non_positive_value_throws(long value)
    {
        var act = () => new CustomerId(value);

        act.Should().Throw<ArgumentException>();
    }
}