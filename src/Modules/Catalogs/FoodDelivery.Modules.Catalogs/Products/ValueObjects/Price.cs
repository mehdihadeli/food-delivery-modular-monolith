using Ardalis.GuardClauses;
using Squirrel.Core.Exception;
using FoodDelivery.Modules.Catalogs.Products.Exceptions.Domain;

namespace FoodDelivery.Modules.Catalogs.Products.ValueObjects;

public record Price
{
    public decimal Value { get; private set; }

    public Price? Null => null;

    public static Price Create(decimal value)
    {
        return new Price
        {
            Value = Guard.Against.NegativeOrZero(
                value,
                exceptionCreator: () => new ProductDomainException("The catalog item price cannot have negative or zero value."))
        };
    }

    public static implicit operator Price(decimal value) => Create(value);

    public static implicit operator decimal(Price value) =>
        Guard.Against.Null(value, exceptionCreator: () => new ProductDomainException("Price can't be null.")).Value;
}
