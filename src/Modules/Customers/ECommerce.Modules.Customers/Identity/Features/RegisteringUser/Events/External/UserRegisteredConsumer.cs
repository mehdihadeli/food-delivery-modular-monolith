using BuildingBlocks.Abstractions.CQRS.Command;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.Context;
using ECommerce.Modules.Customers.Customers.Features.CreatingCustomer;
using ECommerce.Modules.Shared.Identity.Users.Events.Integration;

namespace ECommerce.Modules.Customers.Identity.Features.RegisteringUser.Events.External;

public class UserRegisteredConsumer : IMessageHandler<UserRegistered>
{
    private readonly ICommandProcessor _commandProcessor;
    private readonly IServiceProvider _serviceProvider;

    public UserRegisteredConsumer(ICommandProcessor commandProcessor, IServiceProvider serviceProvider)
    {
        _commandProcessor = commandProcessor;
        _serviceProvider = serviceProvider;
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
