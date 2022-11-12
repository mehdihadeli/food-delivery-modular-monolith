using BuildingBlocks.Core.Exception.Types;
using ECommerce.Modules.Identity.Shared.Models;

namespace ECommerce.Modules.Identity.Identity.Features.RefreshingToken;

public class InvalidRefreshTokenException : BadRequestException
{
    public InvalidRefreshTokenException(Shared.Models.RefreshToken? refreshToken) : base($"refresh token {refreshToken?.Token} is invalid!")
    {
    }
}
