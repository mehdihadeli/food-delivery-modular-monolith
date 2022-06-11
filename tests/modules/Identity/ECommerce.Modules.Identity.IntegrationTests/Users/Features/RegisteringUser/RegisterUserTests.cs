using Bogus;
using ECommerce.Api;
using ECommerce.Modules.Identity.Users.Features.GettingUserById;
using ECommerce.Modules.Identity.Users.Features.RegisteringUser;
using ECommerce.Modules.Identity.Users.Features.RegisteringUser.Events.Integration;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Tests.Shared.Fixtures;
using Xunit.Abstractions;

namespace ECommerce.Modules.Identity.IntegrationTests.Users.Features.RegisteringUser;

public class RegisterUserTests : ModuleTestBase<Program, IdentityModuleConfiguration>
{
    private static RegisterUser _registerUser;

    public RegisterUserTests(IntegrationTestFixture<Program> integrationTestFixture, ITestOutputHelper outputHelper) :
        base(integrationTestFixture, outputHelper)
    {
        // Arrange
        _registerUser = new Faker<RegisterUser>().CustomInstantiator(faker =>
                new RegisterUser(
                    faker.Person.FirstName,
                    faker.Person.LastName,
                    faker.Person.UserName,
                    faker.Person.Email,
                    "123456",
                    "123456"))
            .Generate();
    }

    protected override void RegisterTestsServices(IServiceCollection services)
    {
    }

    [Fact]
    public async Task register_new_user_command_should_persist_new_user_in_db()
    {
        // Act
        var result = await ModuleFixture.GatewayProcessor.SendCommandAsync(_registerUser, CancellationToken);

        // Assert
        result.UserIdentity.Should().NotBeNull();

        // var user = await IdentityModule.FindWriteAsync<ApplicationUser>(result.UserIdentity.Id);
        // user.Should().NotBeNull();

        UserByIdResponse userByIdResponse =
            await ModuleFixture.GatewayProcessor.SendQueryAsync(new GetUserById(result.UserIdentity.Id));

        userByIdResponse.IdentityUser.Should().NotBeNull();
        userByIdResponse.IdentityUser.Id.Should().Be(result.UserIdentity.Id);
    }

    [Fact]
    public async Task register_new_user_command_should_publish_message_to_broker()
    {
        // Arrange
        var shouldPublish = await ModuleFixture.ShouldPublish<UserRegistered>();
        var shouldConsumeWithNewConsumer = await ModuleFixture.ShouldConsumeWithNewConsumer<UserRegistered>();

        // Act
        await ModuleFixture.GatewayProcessor.SendCommandAsync(_registerUser, CancellationToken);

        // Assert
        await shouldPublish.Validate(60.Seconds());
        await shouldConsumeWithNewConsumer.Validate(60.Seconds());
    }
}
