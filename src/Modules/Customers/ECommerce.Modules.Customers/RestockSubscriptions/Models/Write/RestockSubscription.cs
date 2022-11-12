using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.Domain;
using BuildingBlocks.Core.Domain;
using BuildingBlocks.Core.Domain.ValueObjects;
using BuildingBlocks.Core.Exception;
using ECommerce.Modules.Customers.Customers.ValueObjects;
using ECommerce.Modules.Customers.RestockSubscriptions.Exceptions.Domain;
using ECommerce.Modules.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.Events.Domain;
using ECommerce.Modules.Customers.RestockSubscriptions.Features.DeletingRestockSubscription;
using ECommerce.Modules.Customers.RestockSubscriptions.Features.ProcessingRestockNotification;
using ECommerce.Modules.Customers.RestockSubscriptions.ValueObjects;

namespace ECommerce.Modules.Customers.RestockSubscriptions.Models.Write;

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
        Guard.Against.Null(id, new RestockSubscriptionDomainException("Id cannot be null"));
        Guard.Against.Null(customerId, new RestockSubscriptionDomainException("CustomerId cannot be null"));
        Guard.Against.Null(
            productInformation,
            new RestockSubscriptionDomainException("ProductInformation cannot be null"));

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
        Email = Guard.Against.Null(email, new RestockSubscriptionDomainException("Email can't be null."));
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
