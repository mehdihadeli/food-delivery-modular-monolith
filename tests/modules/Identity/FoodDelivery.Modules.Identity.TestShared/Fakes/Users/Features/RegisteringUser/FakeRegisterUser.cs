using Bogus;
using FoodDelivery.Modules.Identity.Users.Features.RegisteringUser;

namespace FoodDelivery.Modules.Identity.TestShared.Fakes.Users.Features.RegisteringUser;

public sealed class FakeRegisterUser : Faker<RegisterUser>
{
    public FakeRegisterUser()
    {
        CustomInstantiator(faker => new RegisterUser(
            faker.Person.FirstName,
            faker.Person.LastName,
            faker.Person.UserName,
            faker.Person.Email,
            "123456",
            "123456"));
    }
}