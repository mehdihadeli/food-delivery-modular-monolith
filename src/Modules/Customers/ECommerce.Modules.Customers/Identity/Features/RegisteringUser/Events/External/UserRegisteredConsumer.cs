using BuildingBlocks.Abstractions.CQRS.Command;
using ECommerce.Modules.Customers.Customers.Features.CreatingCustomer;
using ECommerce.Modules.Shared.Identity.Users.Events.Integration;
using MassTransit;

namespace ECommerce.Modules.Customers.Identity.Features.RegisteringUser.Events.External;

public class UserRegisteredConsumer : IConsumer<UserRegistered>
{
    private readonly ICommandProcessor _commandProcessor;

    public UserRegisteredConsumer(ICommandProcessor commandProcessor)
    {
        _commandProcessor = commandProcessor;
    }

    public async Task Consume(ConsumeContext<UserRegistered> context)
    {
        var userRegistered = context.Message;
        if (userRegistered.Roles is null || !userRegistered.Roles.Contains(CustomersConstants.Role.User))
            return;

        await _commandProcessor.SendAsync(new CreateCustomer(userRegistered.Email));
    }
}
