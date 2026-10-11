using Squirrel.Core.Exception;

namespace FoodDelivery.Modules.Identity.Shared.Exceptions;

public class UserNotFoundException : NotFoundException
{
    public UserNotFoundException(string emailOrUserName) : base($"User with email or username: '{emailOrUserName}' not found.")
    {
    }

    public UserNotFoundException(Guid id) : base($"User with id: '{id}' not found.")
    {
    }
}
