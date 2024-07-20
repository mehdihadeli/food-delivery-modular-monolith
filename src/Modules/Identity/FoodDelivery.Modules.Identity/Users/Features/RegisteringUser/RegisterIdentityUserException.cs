using BuildingBlocks.Core.Exception.Types;

namespace FoodDelivery.Modules.Identity.Users.Features.RegisteringUser;

public class RegisterIdentityUserException : BadRequestException
{
    public RegisterIdentityUserException(string error) : base(error)
    {
    }
}
