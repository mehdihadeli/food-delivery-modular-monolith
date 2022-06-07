// using Bogus;
// using BuildingBlocks.Abstractions.Messaging;
// using BuildingBlocks.Abstractions.Messaging.Context;
// using BuildingBlocks.Abstractions.Web.Module;
// using ECommerce.Modules.Customers;
// using ECommerce.Modules.Customers.Users.Features.RegisteringUser.Events.External;
// using ECommerce.Modules.Identity.Users.Features.RegisteringUser;
// using Humanizer;
// using Hypothesist;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.DependencyInjection.Extensions;
// using Tests.Shared.Fixtures;
// using Xunit.Abstractions;
//
// namespace BuildingBlocks.Core.IntegrationTests;
//
// public class BusPublishTests : IntegrationTestBase<ECommerce.Api.Program>
// {
//     public BusPublishTests(
//         IntegrationTestFixture<ECommerce.Api.Program> integrationTestFixture,
//         ITestOutputHelper outputHelper) :
//         base(integrationTestFixture, outputHelper)
//     {
//     }
//
//     protected override void RegisterTestsServices(IServiceCollection services)
//     {
//     }
//
//     protected override void RegisterModulesTestsServices(IServiceCollection services, IModuleDefinition module)
//     {
//         base.RegisterModulesTestsServices(services, module);
//         if (module.GetType().IsAssignableTo(typeof(IModuleDefinition)))
//         {
//             services.Replace(new ServiceDescriptor(typeof(IHttpClientFactory),
//                 new DelegateHttpClientFactory(_ => GuestClient)));
//         }
//     }
//
//     [Fact]
//     public async Task register_new_user_command_should_send_message_to_correct_exchange_and_consumer()
//     {
//         var newUser = new Faker<RegisterUser>().CustomInstantiator(faker =>
//                 new RegisterUser(
//                     faker.Person.FirstName,
//                     faker.Person.LastName,
//                     faker.Person.UserName,
//                     faker.Person.Email,
//                     "123456",
//                     "123456"))
//             .Generate();
//
//
//         var message = new UserRegistered(
//             Guid.Empty,
//             newUser.Email,
//             newUser.UserName,
//             newUser.FirstName,
//             newUser.LastName,
//             new List<string> {"user"}
//         );
//
//         var hypothesis = Hypothesis
//             .For<UserRegistered>()
//             .Any(x => x.Email == message.Email && x.UserName == message.UserName);
//
//         // CustomersModule.Bus.Consume(hypothesis.AsMessageHandler());
//
//         CustomersModule.Bus.Consume<UserRegistered>(async (consumeContext, ct) =>
//         {
//             await hypothesis.Test(consumeContext.Message);
//         });
//
//         // should receive in customer service.
//         var result = await IdentityModule.GatewayProcessor.ExecuteCommand(
//             async commandProcessor => await commandProcessor.SendAsync(newUser, CancellationToken));
//
//         await hypothesis.Validate(60.Seconds());
//     }
//
//     // [Fact]
//     // async Task factory_with_host_test()
//     // {
//     //     var factory = new WebApplicationFactoryWithHost<Dummy>
//     //     (
//     //         configureServices: services =>
//     //         {
//     //             //configure services as needed
//     //         },
//     //         configure: app =>
//     //         {
//     //             //rest of the required app configuration here
//     //         }
//     //     );
//     //
//     //     var client = factory.CreateClient();
//     //     var response = await client.GetAsync("https://google.com");
//     //
//     //     Assert.True(response.IsSuccessStatusCode);
//     //     //more assertions to validate the culture settings
//     // }
// }
//
// public class Dummy
// {
// }
