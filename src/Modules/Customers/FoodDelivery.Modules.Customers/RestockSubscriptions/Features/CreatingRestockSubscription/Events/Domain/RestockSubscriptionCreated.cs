using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Command;
using BuildingBlocks.Abstractions.CQRS.Event.Internal;
using BuildingBlocks.Core.CQRS.Event.Internal;
using BuildingBlocks.Core.Exception;
using FoodDelivery.Modules.Customers.Customers.Exceptions.Application;
using FoodDelivery.Modules.Customers.RestockSubscriptions.Models.Write;
using FoodDelivery.Modules.Customers.Shared.Data;
using FoodDelivery.Modules.Customers.Shared.Extensions;

namespace FoodDelivery.Modules.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.Events.Domain;

public record RestockSubscriptionCreated(RestockSubscription RestockSubscription) : DomainEvent
{
    public CreateMongoRestockSubscriptionReadModels ToCreateMongoRestockSubscriptionReadModels(
        long customerId,
        string customerName)
    {
        return new CreateMongoRestockSubscriptionReadModels(
            RestockSubscription.Id,
            customerId,
            customerName,
            RestockSubscription.ProductInformation.Id,
            RestockSubscription.ProductInformation.Name,
            RestockSubscription.Email.Value,
            RestockSubscription.Created,
            RestockSubscription.Processed,
            RestockSubscription.ProcessedTime);
    }
}

internal class RestockSubscriptionCreatedHandler : IDomainEventHandler<RestockSubscriptionCreated>
{
    private readonly ICommandProcessor _commandProcessor;
    private readonly CustomersDbContext _customersDbContext;

    public RestockSubscriptionCreatedHandler(ICommandProcessor commandProcessor, CustomersDbContext customersDbContext)
    {
        _commandProcessor = commandProcessor;
        _customersDbContext = customersDbContext;
    }

    public async Task Handle(RestockSubscriptionCreated notification, CancellationToken cancellationToken)
    {
        Guard.Against.Null(notification, nameof(notification));

        var customer = await _customersDbContext.FindCustomerByIdAsync(notification.RestockSubscription.CustomerId);

        Guard.Against.NotFound(
            customer,
            new CustomerNotFoundException(notification.RestockSubscription.CustomerId));

        var mongoReadCommand =
            notification.ToCreateMongoRestockSubscriptionReadModels(customer!.Id, customer.Name.FullName);

        // https://github.com/kgrzybek/modular-monolith-with-ddd#38-internal-processing
        // Schedule multiple read sides to execute here
        await _commandProcessor.ScheduleAsync(new IInternalCommand[] { mongoReadCommand }, cancellationToken);
    }
}
