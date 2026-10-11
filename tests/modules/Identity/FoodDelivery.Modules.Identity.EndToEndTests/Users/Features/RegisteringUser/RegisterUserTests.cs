using System.Net;
using System.Net.Http.Json;
using FoodDelivery.Modules.Identity.Shared.Models;
using FoodDelivery.Modules.Identity.Users.Features.GettingUserById;
using FoodDelivery.Modules.Identity.Users.Features.RegisteringUser;
using Tests.Shared.Fixtures;
using FoodDelivery.Modules.Identity.Shared.Data;

namespace FoodDelivery.Modules.Identity.EndToEndTests.Users.Features.RegisteringUser;

public class RegisterUserTests : IdentityModuleEndToEndTestBase
{
    public RegisterUserTests(
        SharedFixtureWithEfCore<global::Program, IdentityContext> sharedFixture) : base(sharedFixture)
    {
    }

    [Fact]
    public async Task registering_user_returns_created_user_and_persists_profile()
    {
        var request = RegisterUserRequestFaker.Generate() with
        {
            FirstName = "Ada",
            LastName = "Lovelace"
        };

        using var response = await HttpClientJsonExtensions.PostAsJsonAsync(
            SharedFixture.GuestClient, "/api/v1/identity/users", request, CancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.Created, "response body: {0}", responseBody);
        var registration = await response.Content.ReadFromJsonAsync<RegisterUserResponse>(CancellationToken);
        registration.Should().NotBeNull();
        var registeredUser = registration!.UserIdentity;
        registeredUser.Should().NotBeNull();
        registeredUser!.Id.Should().NotBeEmpty();
        registeredUser.FirstName.Should().Be("Ada");
        registeredUser.LastName.Should().Be("Lovelace");
        registeredUser.Email.Should().Be(request.Email);
        registeredUser.UserName.Should().Be(request.UserName);
        registeredUser.UserState.Should().Be(UserState.Active);
        registeredUser.Roles.Should().Equal("user");

        using var getResponse = await SharedFixture.GuestClient.GetAsync($"/api/v1/identity/users/{registeredUser.Id}", CancellationToken);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var profile = await getResponse.Content.ReadFromJsonAsync<UserByIdResponse>(CancellationToken);
        profile.Should().NotBeNull();
        profile!.IdentityUser.Should().NotBeNull();
        profile.IdentityUser!.Id.Should().Be(registeredUser.Id);
        profile.IdentityUser.FirstName.Should().Be("Ada");
        profile.IdentityUser.LastName.Should().Be("Lovelace");
        profile.IdentityUser.Email.Should().Be(request.Email);
        profile.IdentityUser.UserName.Should().Be(request.UserName);
        profile.IdentityUser.UserState.Should().Be(UserState.Active);
    }
}
