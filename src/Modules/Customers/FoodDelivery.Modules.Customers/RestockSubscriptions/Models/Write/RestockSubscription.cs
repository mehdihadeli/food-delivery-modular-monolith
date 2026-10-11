using Ardalis.GuardClauses;
using Squirrel.Abstractions.Domain;
using Squirrel.Core.Domain;
using Squirrel.Core.Domain.ValueObjects;
using Squirrel.Core.Exception;
using FoodDelivery.Modules.Customers.Customers.ValueObjects;
using FoodDelivery.Modules.Customers.RestockSubscriptions.Exceptions.Domain;
using FoodDelivery.Modules.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.Events.Domain;
using FoodDelivery.Modules.Customers.RestockSubscriptions.Features.DeletingRestockSubscription;
using FoodDelivery.Modules.Customers.RestockSubscriptions.Features.ProcessingRestockNotification;
using FoodDelivery.Modules.Customers.RestockSubscriptions.ValueObjects;

namespace FoodDelivery.Modules.Customers.RestockSubscriptions.Models.Write;

public class RestockSubscription : Aggregate<RestockSubscriptionId>, IHaveSoftDelete
{
    public CustomerId CustomerId { get; private set; } = default!;
    public Email Email { get; private set; } = default!;
    public ProductInformation ProductInformation { get; private set; } = default!;
    public bool Processed { get; private set; }
    public DateTime? ProcessedTime { get; private set; }

    public static RestockSubscription Create(
        RestockSubscriptionId id,
        CustomerId customerId,
        ProductInformation productInformation,
        Email email)
    {
        Guard.Against.Null(id, exceptionCreator: () => new RestockSubscriptionDomainException("Id cannot be null"));
        Guard.Against.Null(customerId, exceptionCreator: () => new RestockSubscriptionDomainException("CustomerId cannot be null"));
        Guard.Against.Null(
            productInformation,
            exceptionCreator: () => new RestockSubscriptionDomainException("ProductInformation cannot be null"));

        var restockSubscription = new RestockSubscription
        {
            Id = id, CustomerId = customerId, ProductInformation = productInformation
        };

        restockSubscription.ChangeEmail(email);

        restockSubscription.AddDomainEvents(new RestockSubscriptionCreated(restockSubscription));

        return restockSubscription;
    }

    public void ChangeEmail(Email email)
    {
        Email = Guard.Against.Null(email, exceptionCreator: () => new RestockSubscriptionDomainException("Email can't be null."));
    }

    public void Delete()
    {
        AddDomainEvents(new RestockSubscriptionDeleted(this));
    }

    public void MarkAsProcessed(DateTime processedTime)
    {
        Processed = true;
        ProcessedTime = processedTime;

        AddDomainEvents(new RestockNotificationProcessed(this));
    }
}
