using Ardalis.GuardClauses;
using AutoMapper;
using Squirrel.Abstractions.Commands;
using Squirrel.Core.Events.Internal;
using FoodDelivery.Modules.Customers.RestockSubscriptions.Models.Write;
using FoodDelivery.Modules.Customers.Shared.Data;

namespace FoodDelivery.Modules.Customers.RestockSubscriptions.Features.DeletingRestockSubscription;

public record RestockSubscriptionDeleted(RestockSubscription RestockSubscription) : DomainEvent;

internal class RestockSubscriptionDeletedHandler : IDomainEventHandler<RestockSubscriptionDeleted>
{
    private readonly ICommandBus _commandBus;
    private readonly IMapper _mapper;
    private readonly CustomersDbContext _customersDbContext;

    public RestockSubscriptionDeletedHandler(
        ICommandBus commandBus,
        IMapper mapper,
        CustomersDbContext customersDbContext)
    {
        _commandBus = commandBus;
        _mapper = mapper;
        _customersDbContext = customersDbContext;
    }

    public async ValueTask Handle(RestockSubscriptionDeleted notification, CancellationToken cancellationToken)
    {
        Guard.Against.Null(notification, nameof(notification));

        // var isDeleted = (bool)_customersDbContext.Entry(notification.RestockSubscription)
        //     .Property("IsDeleted")
        //     .CurrentValue!;

        // https://github.com/kgrzybek/modular-monolith-with-ddd#38-internal-processing
        await _commandBus.SendAsync(
            new UpdateMongoRestockSubscriptionReadModel(notification.RestockSubscription, true),
            cancellationToken);
    }
}
