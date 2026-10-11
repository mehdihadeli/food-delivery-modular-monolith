using Squirrel.Abstractions.Messages;
using Squirrel.Core.Extensions;
using Squirrel.Core.Types.Extensions;
using Hypothesist;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Shared.Fixtures;

public static class HandlerFactory
{
    public static IMessageHandler<T> AsMessageHandler<T>(this IHypothesis<T> hypothesis) where T : class, IMessage
    {
        return new SimpleMessageConsumer<T>(hypothesis);
    }

    public static IMessageHandler<TMessage> AsMessageHandler<TMessage, TMessageHandler>(
        this IHypothesis<TMessage> hypothesis, IServiceProvider serviceProvider)
        where TMessage : class, IMessage
        where TMessageHandler : IMessageHandler<TMessage>
    {
        return new MessageConsumer<TMessage>(hypothesis, serviceProvider, typeof(TMessageHandler));
    }
}

internal class MessageConsumer<T> : IMessageHandler<T>
    where T : class, IMessage
{
    private readonly IHypothesis<T> _hypothesis;
    private readonly IServiceProvider _serviceProvider;
    private readonly Type _internalHandler;

    public MessageConsumer(IHypothesis<T> hypothesis, IServiceProvider serviceProvider, Type internalHandler)
    {
        _hypothesis = hypothesis;
        _serviceProvider = serviceProvider;
        _internalHandler = internalHandler;
    }

    public async Task Handle(T message, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetService(_internalHandler);
        if (handler is null)
        {
            await _hypothesis.Test(null!, cancellationToken);
            return;
        }

        await handler.InvokeMethodWithoutResultAsync("Handle", message, cancellationToken);
        await _hypothesis.Test(message);
    }
}

internal class SimpleMessageConsumer<T> : IMessageHandler<T>
    where T : class, IMessage
{
    private readonly IHypothesis<T> _hypothesis;

    public SimpleMessageConsumer(IHypothesis<T> hypothesis)
    {
        _hypothesis = hypothesis;
    }

    public Task Handle(T message, CancellationToken cancellationToken = default)
    {
        return _hypothesis.Test(message);
    }
}
