using Bogus;
using FoodDelivery.Modules.Customers.Users.Features.RegisteringUser.Events.External;

namespace FoodDelivery.Modules.Customers.TestShared.Fakes.Users.Features.RegisteringUser;

public sealed class FakeUserRegistered : Faker<UserRegistered>
{
    public FakeUserRegistered()
    {
        CustomInstantiator(faker => new UserRegistered(
            Guid.NewGuid(),
            faker.Person.Email,
            faker.Person.UserName,
            faker.Person.FirstName,
            faker.Person.LastName,
            new List<string> { "user" }));
    }
}
