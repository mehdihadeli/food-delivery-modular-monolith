using Ardalis.GuardClauses;
using Squirrel.Abstractions.Commands;
using Squirrel.Core.Exception;
using FoodDelivery.Modules.Customers.RestockSubscriptions.Exceptions.Domain;
using FoodDelivery.Modules.Customers.RestockSubscriptions.Features.SendingRestockNotification;
using FoodDelivery.Modules.Customers.Shared.Data;
using FluentValidation;

namespace FoodDelivery.Modules.Customers.RestockSubscriptions.Features.ProcessingRestockNotification;

public record ProcessRestockNotification(long ProductId, int CurrentStock) : ICommand;

internal class ProcessRestockNotificationValidator : AbstractValidator<ProcessRestockNotification>
{
    public ProcessRestockNotificationValidator()
    {
        RuleFor(x => x.CurrentStock)
            .NotEmpty();

        RuleFor(x => x.ProductId)
            .NotEmpty();
    }
}

internal class ProcessRestockNotificationHandler : ICommandHandler<ProcessRestockNotification>
{
    private readonly CustomersDbContext _customersDbContext;
    private readonly ICommandBus _commandBus;
    private readonly ILogger<ProcessRestockNotificationHandler> _logger;

    public ProcessRestockNotificationHandler(
        CustomersDbContext customersDbContext,
        ICommandBus commandBus,
        ILogger<ProcessRestockNotificationHandler> logger)
    {
        _customersDbContext = customersDbContext;
        _commandBus = commandBus;
        _logger = logger;
    }

    public async ValueTask<Unit> Handle(ProcessRestockNotification command, CancellationToken cancellationToken)
    {
        Guard.Against.Null(command, exceptionCreator: () => new RestockSubscriptionDomainException("Command cannot be null"));

        var subscribedCustomers =
            _customersDbContext.RestockSubscriptions.Where(x =>
                x.ProductInformation.Id == command.ProductId && !x.Processed);

        if (!subscribedCustomers.Any())
            return Unit.Value;

        foreach (var restockSubscription in subscribedCustomers)
        {
            restockSubscription!.MarkAsProcessed(DateTime.Now);

            // https://github.com/kgrzybek/modular-monolith-with-ddd#38-internal-processing
            // schedule `SendRestockNotification` for running as a internal command after commenting transaction
            await _commandBus.ScheduleAsync(
                new SendRestockNotification(restockSubscription.Id, command.CurrentStock),
                cancellationToken);
        }

        await _customersDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Marked restock subscriptions as processed");

        return Unit.Value;
    }
}
