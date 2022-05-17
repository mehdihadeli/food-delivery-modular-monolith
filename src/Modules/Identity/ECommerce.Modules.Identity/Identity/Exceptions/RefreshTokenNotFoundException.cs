using BuildingBlocks.Core.Exception.Types;
using ECommerce.Modules.Identity.Shared.Models;

namespace ECommerce.Modules.Identity.Identity.Exceptions;

public class RefreshTokenNotFoundException : NotFoundException
{
    public RefreshTokenNotFoundException(RefreshToken? refreshToken) : base("Refresh token not found.")
    {
    }
}
