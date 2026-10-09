using Ardalis.GuardClauses;
using Squirrel.Abstractions.Commands;
using Squirrel.Core.Events.Internal;
using FoodDelivery.Modules.Customers.RestockSubscriptions.Exceptions.Domain;
using FoodDelivery.Modules.Customers.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Modules.Customers.RestockSubscriptions.Features.DeletingRestockSubscriptionsByTime;

public record DeleteRestockSubscriptionsByTime(DateTime? From = null, DateTime? To = null) : ICommand;

internal class DeleteRestockSubscriptionsByTimeHandler : ICommandHandler<DeleteRestockSubscriptionsByTime>
{
    private readonly CustomersDbContext _customersDbContext;
    private readonly IDomainEventPublisher _domainEventPublisher;
    private readonly ICommandBus _commandBus;
    private readonly ILogger<DeleteRestockSubscriptionsByTimeHandler> _logger;

    public DeleteRestockSubscriptionsByTimeHandler(
        CustomersDbContext customersDbContext,
        IDomainEventPublisher domainEventPublisher,
        ICommandBus commandBus,
        ILogger<DeleteRestockSubscriptionsByTimeHandler> logger)
    {
        _customersDbContext = customersDbContext;
        _domainEventPublisher = domainEventPublisher;
        _commandBus = commandBus;
        _logger = logger;
    }

    public async ValueTask<Unit> Handle(DeleteRestockSubscriptionsByTime command, CancellationToken cancellationToken)
    {
        Guard.Against.Null(command, nameof(command));

        var exists = await _customersDbContext.RestockSubscriptions
            .Where(x => (command.From == null && command.To == null) ||
                        (command.From == null && x.Created <= command.To) ||
                        (command.To == null && x.Created >= command.From) ||
                        (x.Created >= command.From && x.Created <= command.To))
            .ToListAsync(cancellationToken: cancellationToken);

        if (exists.Any() == false)
            throw new RestockSubscriptionDomainException("Not found any items to delete");

        // instead of directly use of `UpdateMongoRestockSubscriptionsReadModelByTime` we can use this code
        // foreach (var restockSubscription in exists)
        // {
        //     restockSubscription.Delete();
        // }

        foreach (var restockSubscription in exists)
        {
            _customersDbContext.Entry(restockSubscription).State = EntityState.Deleted;
            _customersDbContext.Entry(restockSubscription.ProductInformation).State = EntityState.Unchanged;
        }

        await _customersDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("'{Count}' RestockSubscriptions removed.'", exists.Count);

        // https://github.com/kgrzybek/modular-monolith-with-ddd#38-internal-processing
        await _commandBus.SendAsync(
            new UpdateMongoRestockSubscriptionsReadModelByTime(command.From, command.To, IsDeleted: true),
            cancellationToken);

        return Unit.Value;
    }
}
