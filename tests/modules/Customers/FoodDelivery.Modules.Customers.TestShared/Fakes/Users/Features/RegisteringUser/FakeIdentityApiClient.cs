using FoodDelivery.Modules.Customers.Shared.Clients.Identity;
using FoodDelivery.Modules.Customers.Shared.Clients.Identity.Dtos;
using FoodDelivery.Modules.Customers.Users.Features.RegisteringUser.Events.External;
using NSubstitute;

namespace FoodDelivery.Modules.Customers.TestShared.Fakes.Users.Features.RegisteringUser;

public static class FakeIdentityApiClient
{
    public static IIdentityApiClient Create(Func<UserRegistered> getUserRegistered)
    {
        var client = Substitute.For<IIdentityApiClient>();
        client.GetUserByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())!
            .Returns(_ =>
            {
                var userRegistered = getUserRegistered();
                return Task.FromResult(new GetUserByEmailResponse(new UserIdentityDto
                {
                    Email = userRegistered.Email,
                    Id = userRegistered.IdentityId,
                    FirstName = userRegistered.FirstName,
                    LastName = userRegistered.LastName,
                    UserName = userRegistered.UserName
                }));
            });
        return client;
    }
}