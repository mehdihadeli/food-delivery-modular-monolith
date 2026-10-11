using FoodDelivery.Modules.Customers.Customers.Features;
using FoodDelivery.Modules.Customers.Customers.Features.CreatingCustomer.Events.Integration;
using FoodDelivery.Modules.Customers.Shared.Clients.Identity;
using FoodDelivery.Modules.Customers.Shared.Data;
using FoodDelivery.Modules.Customers.TestShared.Fakes.Users.Features.RegisteringUser;
using FoodDelivery.Modules.Customers.Users.Features.RegisteringUser.Events.External;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Tests.Shared.Fixtures;
using Squirrel.Core.Domain.ValueObjects;
using Xunit;

namespace FoodDelivery.Modules.Customers.IntegrationTests.Users.Features.RegisteringUser.Events.External;

public class
    UserRegisteredTests : CustomerModuleTestIntegrationTestBase
{
    private UserRegistered _userRegistered = new FakeUserRegistered().Generate();

    public UserRegisteredTests(
        SharedFixtureWithEfCoreAndMongo<global::Program, CustomersDbContext, CustomersReadDbContext> sharedFixture
    ) : base(sharedFixture)
    {
    }

    protected override void SetupTestConfigureServices(IServiceCollection services)
    {
        services.TryAddSingleton<UserRegisteredTestState>();
        services.Replace(ServiceDescriptor.Transient<IIdentityApiClient>(provider =>
            FakeIdentityApiClient.Create(() => provider.GetRequiredService<UserRegisteredTestState>().Current)));
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        SharedFixture.TestServiceProvider.GetRequiredService<UserRegisteredTestState>().Current = _userRegistered;
    }

    public sealed class UserRegisteredTestState
    {
        public UserRegistered Current { get; set; } = null!;
    }

    [Fact]
    public async Task user_registered_message_should_consume_existing_consumer_by_broker()
    {
        // Arrange
        // Act
        await SharedFixture.PublishMessageAsync(_userRegistered, CancellationToken);

        // Assert
        await SharedFixture.ShouldConsuming<UserRegistered>();
    }

    [Fact]
    public async Task user_registered_message_should_consume_new_consumers_by_broker()
    {
        // Arrange
        // Act
        await SharedFixture.PublishMessageAsync(_userRegistered, CancellationToken);

        // Assert
        await SharedFixture.ShouldConsuming<UserRegistered>();
    }

    [Fact]
    public async Task user_registered_message_should_consume_by_user_registered_consumer()
    {
        // Act
        await SharedFixture.PublishMessageAsync(_userRegistered, CancellationToken);

        // Assert
        await SharedFixture.ShouldConsuming<UserRegistered>();
    }

    [Fact]
    public async Task user_registered_message_should_create_new_customer_in_postgres_write_db()
    {
        // Act
        await SharedFixture.PublishMessageAsync(_userRegistered, CancellationToken);

        // Assert
        await SharedFixture.ShouldConsuming<UserRegistered>();

        var email = Email.Of(_userRegistered.Email);
        var customer = await SharedFixture.ExecuteEfDbContextAsync(async ctx =>
        {
            var res = await EntityFrameworkQueryableExtensions.AnyAsync(ctx.Customers,
            x => x.Email == email);

            return res;
        });

        customer.Should().BeTrue();
    }


    [Fact]
    public async Task user_registered_message_should_create_new_customer_in_mongo()
    {
        // Act
        await SharedFixture.PublishMessageAsync(_userRegistered, CancellationToken);

        // Assert
        var existsCustomer = await SharedFixture.ExecuteMongoDbContextAsync(async ctx =>
        {
            var res = await MongoQueryable.AnyAsync(ctx.Customers.AsQueryable(),
                x => x.Email == _userRegistered.Email);

            return res;
        });

        existsCustomer.Should().BeTrue();
    }

    [Fact]
    public async Task user_registered_should_should_publish_customer_created()
    {
        // Arrange
        // Act
        await SharedFixture.PublishMessageAsync(_userRegistered, CancellationToken);

        // Assert
        await SharedFixture.ShouldSending<CustomerCreated>();
    }
}
