using Bogus;
using FoodDelivery.Modules.Identity.Users.Features.RegisteringUser;

namespace FoodDelivery.Modules.Identity.TestShared.Fakes.Users.Features.RegisteringUser;

public sealed class FakeRegisterUserRequest : Faker<RegisterUserRequest>
{
    public FakeRegisterUserRequest()
    {
        CustomInstantiator(faker => new RegisterUserRequest(
            faker.Person.FirstName,
            faker.Person.LastName,
            faker.Person.UserName,
            faker.Person.Email,
            "123456",
            "123456"));
    }
}