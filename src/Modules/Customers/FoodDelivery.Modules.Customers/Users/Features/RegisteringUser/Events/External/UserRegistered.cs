using Squirrel.Abstractions.Commands;
using Squirrel.Abstractions.Messages;
using Squirrel.Abstractions.Persistence;
using Squirrel.Core.Messages;
using FoodDelivery.Modules.Customers.Customers.Features.CreatingCustomer;

namespace FoodDelivery.Modules.Customers.Users.Features.RegisteringUser.Events.External;

public record UserRegistered(
    Guid IdentityId,
    string Email,
    string UserName,
    string FirstName,
    string LastName,
    List<string>? Roles) : IntegrationEvent, ITxRequest;

public class UserRegisteredConsumer : IMessageHandler<UserRegistered>
{
    private readonly ICommandBus _commandBus;

    public UserRegisteredConsumer(ICommandBus commandBus)
    {
        _commandBus = commandBus;
    }

    public async Task Handle(
        UserRegistered userRegistered,
        CancellationToken cancellationToken = default)
    {
        if (userRegistered.Roles is null || !userRegistered.Roles.Contains(CustomersConstants.Role.User))
            return;

        await _commandBus.SendAsync(new CreateCustomer(userRegistered.Email), cancellationToken);
    }
}
