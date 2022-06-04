using BuildingBlocks.Abstractions.CQRS.Command;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.Context;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Messaging;
using ECommerce.Modules.Customers.Customers.Features.CreatingCustomer;

namespace ECommerce.Modules.Customers.Users.Features.RegisteringUser.Events.External;

public record UserRegistered(
    Guid IdentityId,
    string Email,
    string UserName,
    string FirstName,
    string LastName,
    List<string>? Roles) : IntegrationEvent, ITxRequest;

public class UserRegisteredConsumer : IMessageHandler<UserRegistered>
{
    private readonly ICommandProcessor _commandProcessor;

    public UserRegisteredConsumer(ICommandProcessor commandProcessor)
    {
        _commandProcessor = commandProcessor;
    }

    public async Task HandleAsync(
        IConsumeContext<UserRegistered> messageContext,
        CancellationToken cancellationToken = default)
    {
        var userRegistered = messageContext.Message;
        if (userRegistered.Roles is null || !userRegistered.Roles.Contains(CustomersConstants.Role.User))
            return;

        await _commandProcessor.SendAsync(new CreateCustomer(userRegistered.Email), cancellationToken);
    }
}
