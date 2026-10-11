using Ardalis.GuardClauses;
using Squirrel.Core.Exception;
using FoodDelivery.Modules.Catalogs.Products.Exceptions.Domain;

namespace FoodDelivery.Modules.Catalogs.Products.ValueObjects;

// https://github.com/NimblePros/ValueObjects
public record Dimensions
{
    public int Height { get; private set; }
    public int Width { get; private set; }
    public int Depth { get; private set; }

    public Dimensions? Null => null;

    public static Dimensions Create(int width, int height, int depth)
    {
        Guard.Against.NegativeOrZero(height, exceptionCreator: () => new ProductDomainException("Height must be greater than zero"));
        Guard.Against.NegativeOrZero(width, exceptionCreator: () => new ProductDomainException("Width must be greater than zero"));
        Guard.Against.NegativeOrZero(depth, exceptionCreator: () => new ProductDomainException("Depth must be greater than zero"));

        return new Dimensions { Height = height, Width = width, Depth = depth, };
    }

    public string FormattedDescription()
    {
        return $"HxWxD: {Height} x {Width} x {Depth}";
    }

    public override string ToString()
    {
        return FormattedDescription();
    }
}
