using Bogus;
using BuildingBlocks.Abstractions.Web.Module;
using ECommerce.Modules.Customers.Shared.Clients.Identity;
using ECommerce.Modules.Customers.Shared.Clients.Identity.Dtos;
using ECommerce.Modules.Customers.Users.Features.RegisteringUser.Events.External;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tests.Shared.Fixtures;
using Xunit.Abstractions;
using Program = ECommerce.Api.Program;

namespace ECommerce.Modules.Customers.IntegrationTests.Users.Features.RegisteringUser.Events.External;

public class UserRegisteredTests : IntegrationTestBase<Program>
{
    private readonly UserRegistered _userRegistered;

    public UserRegisteredTests(IntegrationTestFixture<Program> integrationTestFixture, ITestOutputHelper outputHelper)
        : base(integrationTestFixture, outputHelper)
    {
        _userRegistered = new Faker<UserRegistered>().CustomInstantiator(faker =>
                new UserRegistered(
                    Guid.NewGuid(),
                    faker.Person.Email,
                    faker.Person.UserName,
                    faker.Person.FirstName,
                    faker.Person.LastName, new List<string> {"user"}))
            .Generate();
    }

    protected override void RegisterModulesTestsServices(IServiceCollection services, IModuleDefinition module)
    {
        base.RegisterModulesTestsServices(services, module);

        if (module.GetType() == typeof(CustomersModuleConfiguration))
        {
            services.Replace(ServiceDescriptor.Transient<IIdentityApiClient>(x =>
            {
                var f = Substitute.For<IIdentityApiClient>();
                f.GetUserByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())!
                    .Returns(x => Task.FromResult(new GetUserByEmailResponse(new UserIdentityDto()
                    {
                        Email = _userRegistered.Email,
                        Id = _userRegistered.IdentityId,
                        FirstName = _userRegistered.FirstName,
                        LastName = _userRegistered.LastName,
                        UserName = _userRegistered.UserName
                    })));

                return f;
            }));
        }
    }

    [Fact]
    public async Task user_registered_message_should_consume_by_broker()
    {
        // Arrange
        var message = _userRegistered;

        var shouldConsume = await CustomersModule.ShouldConsumed<UserRegistered>();

        // Act
        await CustomersModule.PublishMessageAsync(message);

        // Assert
        await shouldConsume.Validate(60.Seconds());
    }

    [Fact]
    public async Task user_registered_message_should_consume_by_user_registered_consumer()
    {
        // Arrange
        var message = _userRegistered;

        var shouldConsume = await CustomersModule.ShouldConsumed<UserRegistered, UserRegisteredConsumer>();

        // Act
        await CustomersModule.PublishMessageAsync(message);

        // Assert
        await shouldConsume.Validate(60.Seconds());
    }
}
