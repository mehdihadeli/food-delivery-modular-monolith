using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.Context;
using ECommerce.Modules.Shared.Identity.Users.Events.Integration;
using Humanizer;
using Hypothesist;
using Tests.Shared.Fixtures;
using Xunit.Abstractions;

namespace BuildingBlocks.Integration.MassTransit.IntegrationTests;

public class BusPublishTests : IntegrationTestBase<ECommerce.Api.Program>
{
    public BusPublishTests(
        IntegrationTestFixture<ECommerce.Api.Program> integrationTestFixture,
        ITestOutputHelper outputHelper) :
        base(integrationTestFixture, outputHelper)
    {
    }

    [Fact]
    public async Task publish_user_registered_should_send_message_to_correct_exchange_and_consumer()
    {
        var message = new UserRegistered(
            Guid.NewGuid(), $"{Guid.NewGuid()}@test.com",
            "ss",
            "ss", "ss",
            new List<string> {"user"}
        );

        var hypothesis = Hypothesis
            .For<UserRegistered>()
            .Any(x => x == message);

        // IntegrationTestFixture.Bus.Consume(hypothesis.AsMessageHandler());

        IntegrationTestFixture.Bus.Consume<UserRegistered>(async (consumeContext, ct) =>
        {
            await hypothesis.Test(consumeContext.Message);
        });

        // should receive in customer service.
        await IntegrationTestFixture.Bus.PublishAsync(message, null, CancellationToken.None);

        await hypothesis.Validate(60.Seconds());
    }
}

public class UserRegisteredHandler : IMessageHandler<UserRegistered>
{
    public Task HandleAsync(IConsumeContext<UserRegistered> messageContext,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
