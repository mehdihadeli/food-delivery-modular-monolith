using BuildingBlocks.Core.Exception.Types;

namespace FoodDelivery.Modules.Identity.Identity.Exceptions;

public class PhoneNumberNotConfirmedException : BadRequestException
{
    public PhoneNumberNotConfirmedException(string phone) : base($"The phone number '{phone}' is not confirmed yet.")
    {
    }
}
