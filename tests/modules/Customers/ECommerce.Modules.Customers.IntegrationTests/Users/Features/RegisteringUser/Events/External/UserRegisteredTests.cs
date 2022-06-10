using Bogus;
using BuildingBlocks.Abstractions.Web.Module;
using ECommerce.Modules.Customers.Customers.Features;
using ECommerce.Modules.Customers.Customers.Features.CreatingCustomer.Events.Integration;
using ECommerce.Modules.Customers.Shared.Clients.Identity;
using ECommerce.Modules.Customers.Shared.Clients.Identity.Dtos;
using ECommerce.Modules.Customers.Shared.Data;
using ECommerce.Modules.Customers.Users.Features.RegisteringUser.Events.External;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tests.Shared.Fixtures;
using Xunit.Abstractions;
using Program = ECommerce.Api.Program;

namespace ECommerce.Modules.Customers.IntegrationTests.Users.Features.RegisteringUser.Events.External;

public class
    UserRegisteredTests : ModuleTestBase<Program, CustomersModuleConfiguration, CustomersDbContext, CustomersReadDbContext>
{
    private static UserRegistered _userRegistered;

    public UserRegisteredTests(IntegrationTestFixture<Program> integrationTestFixture, ITestOutputHelper outputHelper) :
        base(integrationTestFixture, outputHelper)
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
                    .Returns(args =>
                    {
                        var email = args.Arg<string>();

                        return Task.FromResult(new GetUserByEmailResponse(new UserIdentityDto()
                        {
                            Email = _userRegistered.Email,
                            Id = _userRegistered.IdentityId,
                            FirstName = _userRegistered.FirstName,
                            LastName = _userRegistered.LastName,
                            UserName = _userRegistered.UserName
                        }));
                    });

                return f;
            }));
        }
    }

    [Fact]
    public async Task user_registered_message_should_consume_existing_consumer_by_broker()
    {
        // Arrange
        var shouldConsume = await ModuleFixture.ShouldConsume<UserRegistered>();

        // Act
        await ModuleFixture.PublishMessageAsync(_userRegistered, null, CancellationToken);

        // Assert
        await shouldConsume.Validate(60.Seconds());
    }

    [Fact]
    public async Task user_registered_message_should_consume_new_consumers_by_broker()
    {
        // Arrange
        var shouldConsume = await ModuleFixture.ShouldConsumeWithNewConsumer<UserRegistered>();

        // Act
        await ModuleFixture.PublishMessageAsync(_userRegistered, cancellationToken: CancellationToken);

        // Assert
        await shouldConsume.Validate(60.Seconds());
    }

    [Fact]
    public async Task user_registered_message_should_consume_by_user_registered_consumer()
    {
        var shouldConsume = await ModuleFixture.ShouldConsume<UserRegistered, UserRegisteredConsumer>();

        // Act
        await ModuleFixture.PublishMessageAsync(_userRegistered, cancellationToken: CancellationToken);

        // Assert
        await shouldConsume.Validate(60.Seconds());
    }

    [Fact]
    public async Task user_registered_message_should_create_new_customer_in_postgres_write_db()
    {
        _userRegistered = new Faker<UserRegistered>().CustomInstantiator(faker =>
                new UserRegistered(
                    Guid.NewGuid(),
                    faker.Person.Email,
                    faker.Person.UserName,
                    faker.Person.FirstName,
                    faker.Person.LastName, new List<string> {"user"}))
            .Generate();

        // Act
        await ModuleFixture.PublishMessageAsync(_userRegistered, cancellationToken: CancellationToken);

        // Assert
        var shouldConsume = await ModuleFixture.ShouldConsume<UserRegistered, UserRegisteredConsumer>(x =>
            x.Email.ToLower() == _userRegistered.Email.ToLower());

        await shouldConsume.Validate(60.Seconds());

        var customer = await ModuleFixture.ExecuteContextAsync(async ctx =>
        {
            var res = await ctx.Customers.AnyAsync(x => x.Email == _userRegistered.Email.ToLower());

            return res;
        });

        customer.Should().BeTrue();
    }


    [Fact]
    public async Task user_registered_message_should_create_new_customer_in_internal_persistence_message_and_mongo()
    {
        // Act
        await ModuleFixture.PublishMessageAsync(_userRegistered, cancellationToken: CancellationToken);

        // Assert
        await ModuleFixture.ShouldProcessedPersistInternalCommand<CreateMongoCustomerReadModels>();

        var existsCustomer = await ModuleFixture.ExecuteReadContextAsync(async ctx =>
        {
            var res = await ctx.Customers.AsQueryable().AnyAsync(x => x.Email == _userRegistered.Email.ToLower());

            return res;
        });

        existsCustomer.Should().BeTrue();
    }

    [Fact]
    public async Task user_registered_message_should_create_customer_created_in_the_outbox()
    {
        // Act
        await ModuleFixture.PublishMessageAsync(_userRegistered, cancellationToken: CancellationToken);

        await ModuleFixture.ShouldProcessedOutboxPersistMessage<CustomerCreated>();
    }

    [Fact]
    public async Task user_registered_should_should_publish_customer_created()
    {
        // Arrange
        var shouldPublish = await ModuleFixture.ShouldPublish<CustomerCreated>();

        // Act
        await ModuleFixture.PublishMessageAsync(_userRegistered, cancellationToken: CancellationToken);

        // Assert
        await shouldPublish.Validate(60.Seconds());
    }
}
