using Ardalis.GuardClauses;
using Squirrel.Core.Exception;
using FoodDelivery.Modules.Customers.Products;
using FoodDelivery.Modules.Customers.RestockSubscriptions.Exceptions.Domain;

namespace FoodDelivery.Modules.Customers.RestockSubscriptions.ValueObjects;

// Here versioning Name is not important for us so we can save it on DB
public record ProductInformation
{
    public string Name { get; private set; } = null!;
    public ProductId Id { get; private set; } = null!;

    public static ProductInformation Create(ProductId id, string name)
    {
        return new ProductInformation
        {
            Name = Guard.Against.NullOrWhiteSpace(
                name,
                exceptionCreator: () => new RestockSubscriptionDomainException("Product name can't be null.")),
            Id = Guard.Against.Null(id, exceptionCreator: () => new RestockSubscriptionDomainException("Product Id can't be  null.")),
        };
    }
}
