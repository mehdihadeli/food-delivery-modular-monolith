using Squirrel.Core.Exception;

namespace FoodDelivery.Modules.Identity.Identity.Features.Login;

public class LoginFailedException : AppException
{
    public LoginFailedException(string userNameOrEmail) : base($"Login failed for username: {userNameOrEmail}")
    {
        UserNameOrEmail = userNameOrEmail;
    }

    public string UserNameOrEmail { get; }
}
