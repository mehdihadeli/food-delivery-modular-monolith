using Squirrel.Core.Exception;

namespace FoodDelivery.Modules.Customers.Customers.Exceptions.Domain;

public class InvalidNameException : BadRequestException
{
    public string Name { get; }

    public InvalidNameException(string name) : base($"Name: '{name}' is invalid.")
    {
        Name = name;
    }
}
