using Squirrel.Core.Exception;

namespace FoodDelivery.Modules.Customers.Customers.Exceptions.Domain;

public class UnsupportedNationalityException : BadRequestException
{
    public string Nationality { get; }

    public UnsupportedNationalityException(string nationality) : base($"Nationality: '{nationality}' is unsupported.")
    {
        Nationality = nationality;
    }
}
