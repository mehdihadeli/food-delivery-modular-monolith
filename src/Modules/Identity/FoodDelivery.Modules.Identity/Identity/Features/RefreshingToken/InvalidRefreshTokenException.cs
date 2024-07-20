using BuildingBlocks.Core.Exception.Types;
using FoodDelivery.Modules.Identity.Shared.Models;

namespace FoodDelivery.Modules.Identity.Identity.Features.RefreshingToken;

public class InvalidRefreshTokenException : BadRequestException
{
    public InvalidRefreshTokenException(Shared.Models.RefreshToken? refreshToken) : base($"refresh token {refreshToken?.Token} is invalid!")
    {
    }
}
