// using Bogus;
// using ECommerce.Api;
// using ECommerce.Modules.Identity.Users.Features.GettingUserById;
// using ECommerce.Modules.Identity.Users.Features.RegisteringUser;
// using ECommerce.Modules.Identity.Users.Features.RegisteringUser.Events.Integration;
// using Humanizer;
// using Microsoft.Extensions.DependencyInjection;
// using Tests.Shared.Fixtures;
// using Xunit.Abstractions;
//
// namespace ECommerce.Modules.Identity.IntegrationTests.Users.Features.RegisteringUser;
//
// public class RegisterUserTests : IntegrationTestBase<Program>
// {
//     private readonly RegisterUser _registerUser;
//
//     public RegisterUserTests(IntegrationTestFixture<Program> integrationTestFixture, ITestOutputHelper outputHelper) :
//         base(integrationTestFixture, outputHelper)
//     {
//         // Arrange
//         _registerUser = new Faker<RegisterUser>().CustomInstantiator(faker =>
//                 new RegisterUser(
//                     faker.Person.FirstName,
//                     faker.Person.LastName,
//                     faker.Person.UserName,
//                     faker.Person.Email,
//                     "123456",
//                     "123456"))
//             .Generate();
//     }
//
//     protected override void RegisterTestsServices(IServiceCollection services)
//     {
//     }
//
//     [Fact]
//     public async Task register_new_user_command_should_persist_new_user_in_db()
//     {
//         // Arrange
//         var newUser = _registerUser;
//         // Act
//         var result = await IdentityModule.GatewayProcessor.ExecuteCommand(
//             async commandProcessor => await commandProcessor.SendAsync(newUser, CancellationToken));
//
//         // Assert
//         result.UserIdentity.Should().NotBeNull();
//
//         // var user = await IdentityModule.FindWriteAsync<ApplicationUser>(result.UserIdentity.Id);
//         // user.Should().NotBeNull();
//
//         UserByIdResponse userByIdResponse =
//             await IdentityModule.GatewayProcessor.ExecuteQuery<GetUserById, UserByIdResponse>(
//                 new GetUserById(result.UserIdentity.Id));
//
//         userByIdResponse.IdentityUser.Should().NotBeNull();
//         userByIdResponse.IdentityUser.Id.Should().Be(result.UserIdentity.Id);
//     }
//
//     [Fact]
//     public async Task register_new_user_command_should_publish_message_to_broker()
//     {
//         // Arrange
//         var newUser = _registerUser;
//
//         var shouldPublish = await IdentityModule.ShouldPublish<UserRegistered>();
//         var shouldConsumeWithNewConsumer = await IdentityModule.ShouldConsumeWithNewConsumer<UserRegistered>();
//
//         // Act
//         await IdentityModule.GatewayProcessor.ExecuteCommand(
//             async commandProcessor => await commandProcessor.SendAsync(newUser, CancellationToken));
//
//         // Assert
//         await shouldPublish.Validate(60.Seconds());
//         await shouldConsumeWithNewConsumer.Validate(60.Seconds());
//     }
// }
