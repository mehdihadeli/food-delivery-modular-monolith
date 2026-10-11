using System.Security.Claims;
using Squirrel.Core.Exception;

namespace FoodDelivery.Modules.Identity.Identity.Exceptions;

public class InvalidTokenException : BadRequestException
{
    public InvalidTokenException(ClaimsPrincipal? claimsPrincipal) : base("access_token is invalid!")
    {
    }
}
