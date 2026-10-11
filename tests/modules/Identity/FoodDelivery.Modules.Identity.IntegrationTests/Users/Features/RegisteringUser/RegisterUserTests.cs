using FoodDelivery.Modules.Identity.TestShared.Fakes.Users.Features.RegisteringUser;
using FoodDelivery.Modules.Identity.Users.Features.GettingUserById;
using FoodDelivery.Modules.Identity.Users.Features.RegisteringUser;
using FoodDelivery.Modules.Identity.Users.Features.RegisteringUser.Events.Integration;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Tests.Shared.Fixtures;
using Xunit;
using FoodDelivery.Modules.Identity.Shared.Data;
using FoodDelivery.Modules.Customers.Shared.Data;
using FoodDelivery.Modules.Customers;
using Squirrel.Abstractions.Web.Module;
using Squirrel.Abstractions.Commands;
using Microsoft.EntityFrameworkCore;
using FoodDelivery.Api.Messaging;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Hosting;
using Wolverine;
using Wolverine.Postgresql;
using Wolverine.RDBMS;
using Wolverine.Runtime;
using Wolverine.ErrorHandling;
using Wolverine.Persistence.Durability.DeadLetterManagement;
using ICommandBus = Squirrel.Abstractions.Commands.ICommandBus;

namespace FoodDelivery.Modules.Identity.IntegrationTests.Users.Features.RegisteringUser;

public class RegisterUserTests : IdentityModuleIntegrationTestBase
{
    private readonly RegisterUser _registerUser = new FakeRegisterUser().Generate();

    public RegisterUserTests(SharedFixtureWithEfCore<global::Program, IdentityContext> sharedFixture) :
        base(sharedFixture)
    {
    }

    protected override void SetupTestConfigureServices(IServiceCollection services)
    {
    }

    [Fact]
    public async Task rolled_back_registration_should_not_create_customer()
    {
        await using var scope = SharedFixture.TestServiceProvider.CreateAsyncScope();
        var identity = scope.ServiceProvider.GetRequiredService<IdentityContext>();
        var deliveryId = Guid.Empty;
        await identity.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            await using var registrationTransaction = await identity.Database.BeginTransactionAsync(CancellationToken);
            await scope.ServiceProvider.GetRequiredService<ICommandBus>()
                .SendAsync<RegisterUserResponse>(_registerUser, CancellationToken);
            deliveryId = scope.ServiceProvider.GetRequiredService<ModuleMessagingSession>().Context.Outstanding
                .Single(envelope => envelope.Message is UserRegistered).Id;
            (await CountPersistedEnvelope(identity, deliveryId)).Should().Be(1);
            await registrationTransaction.RollbackAsync(CancellationToken);
        });

        deliveryId.Should().NotBeEmpty();
        (await CountPersistedEnvelope(identity, deliveryId)).Should().Be(0);

        var customersRoot = SharedFixture.ServiceProvider.GetServices<ICompositionRoot>()
            .Single(root => root.ModuleDefinition is CustomersModuleConfiguration);
        await using var customersScope = customersRoot.ServiceProvider.CreateAsyncScope();
        var customers = customersScope.ServiceProvider.GetRequiredService<CustomersDbContext>();
        (await customers.Customers.AnyAsync(customer => customer.IdentityId == _registerUser.Id, CancellationToken))
            .Should().BeFalse();
        (await identity.Users.AnyAsync(user => user.Id == _registerUser.Id, CancellationToken)).Should().BeFalse();
    }

    [Fact]
    public async Task committed_unflushed_envelope_should_recover_in_new_runtime()
    {
        var schema = "recovery_" + Guid.NewGuid().ToString("N");
        var message = new RecoveryProbe(Guid.NewGuid());
        var sourceRecorder = new RecoveryRecorder();
        var deliveryId = Guid.Empty;
        await using var scope = SharedFixture.TestServiceProvider.CreateAsyncScope();
        var identity = scope.ServiceProvider.GetRequiredService<IdentityContext>();
        using (var source = CreateRecoveryRuntime(schema, sourceRecorder))
        {
            await source.StartAsync(CancellationToken);
            var runtime = source.Services.GetRequiredService<IWolverineRuntime>();
            await identity.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
            {
                await using var transaction = await identity.Database.BeginTransactionAsync(CancellationToken);
                var messaging = new MessageContext(runtime);
                await messaging.EnlistInOutboxAsync(new DatabaseEnvelopeTransaction(
                    (IMessageDatabase)runtime.Storage, transaction.GetDbTransaction()));
                await messaging.PublishAsync(message);
                deliveryId = messaging.Outstanding.Single().Id;
                (await CountPersistedEnvelope(identity, deliveryId, schema)).Should().Be(1);
                await source.StopAsync(CancellationToken);
                await transaction.CommitAsync(CancellationToken);
            });
        }

        sourceRecorder.Deliveries.Should().BeEmpty();
        (await CountPersistedEnvelope(identity, deliveryId, schema)).Should().Be(1);
        var receiverRecorder = new RecoveryRecorder();
        using var receiver = CreateRecoveryRuntime(schema, receiverRecorder);
        await receiver.StartAsync(CancellationToken);
        try
        {
            await SharedFixture.WaitUntilConditionMet(() => Task.FromResult(receiverRecorder.Deliveries.Count == 1));
            receiverRecorder.Deliveries.Should().ContainSingle()
                .Which.Should().Be((message.Id, deliveryId));
            await SharedFixture.WaitUntilConditionMet(async () =>
                await identity.Database.SqlQueryRaw<string>(
                    $"SELECT status AS \"Value\" FROM {schema}.wolverine_incoming_envelopes WHERE id = {{0}}", deliveryId)
                    .SingleAsync(CancellationToken) == "Handled");
        }
        finally
        {
            await receiver.StopAsync(CancellationToken);
        }
    }

    [Fact]
    public async Task failed_delivery_should_dead_letter_and_replay_with_same_envelope_id()
    {
        var schema = "replay_" + Guid.NewGuid().ToString("N");
        var message = new RecoveryProbe(Guid.NewGuid());
        var recorder = new RecoveryRecorder { Fail = true };
        using var receiver = CreateRecoveryRuntime(schema, recorder);
        await receiver.StartAsync(CancellationToken);
        try
        {
            var runtime = receiver.Services.GetRequiredService<IWolverineRuntime>();
            await using var scope = receiver.Services.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<Wolverine.IMessageBus>().PublishAsync(message);
            await SharedFixture.WaitUntilConditionMet(async () => recorder.Attempts == 3
                && await runtime.Storage.DeadLetters.DeadLetterEnvelopeByIdAsync(recorder.LastEnvelopeId) is not null);
            recorder.Deliveries.Should().BeEmpty();
            var deliveryId = recorder.LastEnvelopeId;
            deliveryId.Should().NotBeEmpty();

            recorder.Fail = false;
            await runtime.Storage.DeadLetters.ReplayAsync(
                new DeadLetterEnvelopeQuery { MessageIds = [deliveryId] }, CancellationToken);
            await SharedFixture.WaitUntilConditionMet(() => Task.FromResult(recorder.Deliveries.Count == 1));
            recorder.Deliveries.Should().ContainSingle().Which.Should().Be((message.Id, deliveryId));
            recorder.Attempts.Should().Be(4);
        }
        finally
        {
            await receiver.StopAsync(CancellationToken);
        }
    }

    [Fact]
    public async Task register_new_user_command_should_persist_new_user_in_db()
    {
        // Act
        var result = await SharedFixture.SendAsync<RegisterUserResponse>(_registerUser, CancellationToken);

        // Assert
        result.UserIdentity.Should().NotBeNull();

        // var user = await IdentityModule.FindWriteAsync<ApplicationUser>(result.UserIdentity.Id);
        // user.Should().NotBeNull();

        UserByIdResponse userByIdResponse =
            await SharedFixture.QueryAsync(new GetUserById(result.UserIdentity.Id));

        userByIdResponse.IdentityUser.Should().NotBeNull();
        userByIdResponse.IdentityUser.Id.Should().Be(result.UserIdentity.Id);
    }

    [Fact]
    public async Task register_new_user_command_should_publish_message_to_broker()
    {
        // Arrange
        // Act
        var result = await SharedFixture.SendAsync<RegisterUserResponse>(_registerUser, CancellationToken);

        // Assert
        await SharedFixture.ShouldSending<UserRegistered>();
        await SharedFixture.ShouldConsuming<UserRegistered>();
        SharedFixture.ServiceProvider.GetService<ICommandBus>().Should().BeNull();
        SharedFixture.ServiceProvider.GetService<CustomersDbContext>().Should().BeNull();
        var customersRoot = SharedFixture.ServiceProvider.GetServices<ICompositionRoot>()
            .Single(root => root.ModuleDefinition is CustomersModuleConfiguration);
        await using var scope = customersRoot.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<CustomersDbContext>();
        var customer = await context.Customers.SingleAsync(
            customer => customer.IdentityId == result.UserIdentity.Id, CancellationToken);
        customer.Email.Value.Should().Be(_registerUser.Email);
    }

    private Task<long> CountPersistedEnvelope(IdentityContext context, Guid deliveryId, string schema = "wolverine") =>
        context.Database.SqlQueryRaw<long>(
            $"SELECT (SELECT count(*) FROM {schema}.wolverine_incoming_envelopes WHERE id = {{0}}) + " +
            $"(SELECT count(*) FROM {schema}.wolverine_outgoing_envelopes WHERE id = {{0}}) AS \"Value\"", deliveryId)
            .SingleAsync(CancellationToken);

    private IHost CreateRecoveryRuntime(string schema, RecoveryRecorder recorder)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddSingleton(recorder);
        builder.Services.AddWolverine(options =>
        {
            options.Discovery.DisableConventionalDiscovery();
            options.Discovery.IncludeType<RecoveryProbeHandler>();
            options.PersistMessagesWithPostgresql(SharedFixture.PostgresContainerFixture.ConnectionString, schema);
            options.Policies.UseDurableLocalQueues();
            options.Policies.OnException<InvalidOperationException>().RetryTimes(2);
            options.Durability.KeepAfterMessageHandling = TimeSpan.FromHours(1);
        });
        return builder.Build();
    }

    public sealed record RecoveryProbe(Guid Id);

    public sealed class RecoveryRecorder
    {
        private int _attempts;
        private volatile bool _fail;
        public bool Fail { get => _fail; set => _fail = value; }
        public int Attempts => Volatile.Read(ref _attempts);
        public Guid LastEnvelopeId { get; private set; }
        public ConcurrentQueue<(Guid MessageId, Guid DeliveryId)> Deliveries { get; } = new();

        public void Record(Guid messageId, Guid deliveryId)
        {
            LastEnvelopeId = deliveryId;
            Interlocked.Increment(ref _attempts);
            if (Fail) throw new InvalidOperationException("Simulated recipient failure.");
            Deliveries.Enqueue((messageId, deliveryId));
        }
    }

    public sealed class RecoveryProbeHandler
    {
        public static void Handle(RecoveryProbe message, Wolverine.Envelope envelope, RecoveryRecorder recorder) =>
            recorder.Record(message.Id, envelope.Id);
    }
}
