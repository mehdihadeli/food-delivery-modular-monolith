using Squirrel.Core.Exception;
using FoodDelivery.Modules.Identity.Shared.Models;

namespace FoodDelivery.Modules.Identity.Identity.Exceptions;

public class RefreshTokenNotFoundException : NotFoundException
{
    public RefreshTokenNotFoundException(RefreshToken? refreshToken) : base("Refresh token not found.")
    {
    }
}
