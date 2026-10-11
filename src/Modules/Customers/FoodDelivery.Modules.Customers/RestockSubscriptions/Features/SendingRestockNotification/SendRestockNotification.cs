using Ardalis.GuardClauses;
using Squirrel.Abstractions.Commands;
using Squirrel.Abstractions.Persistence;
using Squirrel.Core.Exception;
using Squirrel.Email;
using Squirrel.Email.Options;
using FoodDelivery.Modules.Customers.RestockSubscriptions.Exceptions.Domain;
using FoodDelivery.Modules.Customers.Shared.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FoodDelivery.Modules.Customers.RestockSubscriptions.Features.SendingRestockNotification;

public record SendRestockNotification(long RestockSubscriptionId, int CurrentStock) : IInternalCommand, ITxRequest
{
    public Guid InternalCommandId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public string Type => GetType().FullName!;
}

internal class SendRestockNotificationValidator : AbstractValidator<SendRestockNotification>
{
    public SendRestockNotificationValidator()
    {
        RuleFor(x => x.RestockSubscriptionId)
            .NotEmpty();

        RuleFor(x => x.CurrentStock)
            .NotEmpty();
    }
}

internal class SendRestockNotificationHandler : ICommandHandler<SendRestockNotification>
{
    private readonly CustomersDbContext _customersDbContext;
    private readonly IEmailSender _emailSender;
    private readonly EmailOptions _emailConfig;
    private readonly ILogger<SendRestockNotificationHandler> _logger;

    public SendRestockNotificationHandler(
        CustomersDbContext customersDbContext,
        IEmailSender emailSender,
        IOptions<EmailOptions> emailConfig,
        ILogger<SendRestockNotificationHandler> logger)
    {
        _customersDbContext = customersDbContext;
        _emailSender = emailSender;
        _emailConfig = emailConfig.Value;
        _logger = logger;
    }

    public async ValueTask<Unit> Handle(SendRestockNotification command, CancellationToken cancellationToken)
    {
        Guard.Against.Null(command, exceptionCreator: () => new RestockSubscriptionDomainException("Command cannot be null"));

        var restockSubscription =
            await _customersDbContext.RestockSubscriptions
                .FirstOrDefaultAsync(x => x.Id == command.RestockSubscriptionId, cancellationToken: cancellationToken);

        if (_emailConfig.Enable && restockSubscription is not null)
        {
            Guard.Against.Null(restockSubscription.Email);

            await _emailSender.SendAsync(
                new EmailObject(
                    restockSubscription.Email!,
                    _emailConfig.From,
                    "Restock Notification",
                    $"Your product {restockSubscription.ProductInformation.Name} is back in stock. Current stock is {command.CurrentStock}"));

            _logger.LogInformation("Restock notification sent to email {Email}", restockSubscription.Email);
        }

        return Unit.Value;
    }
}
