using BuildingBlocks.Core.Exception.Types;

namespace FoodDelivery.Modules.Identity.Identity.Exceptions;

public class PasswordIsInvalidException : AppException
{
    public PasswordIsInvalidException(string message) : base(message)
    {
    }
}
