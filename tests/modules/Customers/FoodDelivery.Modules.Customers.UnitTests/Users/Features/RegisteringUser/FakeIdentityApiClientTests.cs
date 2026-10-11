using FoodDelivery.Modules.Customers.TestShared.Fakes.Users.Features.RegisteringUser;
using FoodDelivery.Modules.Customers.UnitTests.Common;

namespace FoodDelivery.Modules.Customers.UnitTests.Users.Features.RegisteringUser;

public class FakeIdentityApiClientTests : CustomerModuleUnitTestBase
{
    [Fact]
    public async Task identity_lookup_maps_registration_event_fields()
    {
        var userRegistered = UserRegisteredFaker.Generate() with
        {
            IdentityId = Guid.Parse("385fc94f-fbd4-4399-a3ea-61d87344910a"),
            FirstName = "Ada",
            LastName = "Lovelace",
            UserName = "ada.lovelace",
            Email = "ada@example.com"
        };
        var client = FakeIdentityApiClient.Create(() => userRegistered);

        var response = await client.GetUserByEmailAsync("ada@example.com", CancellationToken.None);

        response.Should().NotBeNull();
        response!.UserIdentity.Should().NotBeNull();
        response.UserIdentity!.Id.Should().Be(Guid.Parse("385fc94f-fbd4-4399-a3ea-61d87344910a"));
        response.UserIdentity.FirstName.Should().Be("Ada");
        response.UserIdentity.LastName.Should().Be("Lovelace");
        response.UserIdentity.UserName.Should().Be("ada.lovelace");
        response.UserIdentity.Email.Should().Be("ada@example.com");
    }

    [Fact]
    public async Task identity_lookup_reads_current_event_after_replacement()
    {
        var userRegistered = UserRegisteredFaker.Generate() with { Email = "first@example.com" };
        var client = FakeIdentityApiClient.Create(() => userRegistered);
        var firstResponse = await client.GetUserByEmailAsync("first@example.com", CancellationToken.None);
        firstResponse!.UserIdentity!.Email.Should().Be("first@example.com");
        userRegistered = userRegistered with
        {
            IdentityId = Guid.Parse("9dbb11a4-1ab7-48ad-9903-fc559e605134"),
            Email = "replacement@example.com"
        };

        var replacementResponse = await client.GetUserByEmailAsync("replacement@example.com", CancellationToken.None);

        replacementResponse!.UserIdentity!.Email.Should().Be("replacement@example.com");
        replacementResponse.UserIdentity.Id.Should().Be(Guid.Parse("9dbb11a4-1ab7-48ad-9903-fc559e605134"));
        firstResponse.UserIdentity.Email.Should().Be("first@example.com");
    }
}