using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.Context;
using Hypothesist;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Shared.Fixtures;

public static class HandlerFactory
{
    public static IMessageHandler<T> AsMessageHandler<T>(this IHypothesis<T> hypothesis) where T : class, IMessage =>
        new TestMessageHandler<T>(hypothesis);


    public static IServiceCollection AddTestsHandlers(this IServiceCollection services)
    {
        services.AddScoped(typeof(IMessageHandler<>), typeof(TestMessageHandler<>));

        return services;
    }
}

public class TestMessageHandler<T> : IMessageHandler<T> where T : class, IMessage
{
    private readonly IHypothesis<T> _hypothesis;

    public TestMessageHandler(IHypothesis<T> hypothesis) =>
        _hypothesis = hypothesis;

    public Task HandleAsync(IConsumeContext<T> messageContext, CancellationToken cancellationToken = default)
    {
        _hypothesis.Test(messageContext.Message, cancellationToken);

        return Task.CompletedTask;
    }
}
