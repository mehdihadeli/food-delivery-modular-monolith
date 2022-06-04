using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.Context;
using BuildingBlocks.Core.Extensions;
using Hypothesist;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Shared.Fixtures;

public static class HandlerFactory
{
    public static MessageHandler<T> AsMessageHandler<T>(this IHypothesis<T> hypothesis) where T : class, IMessage
    {
        return (context, cancellationToken) => hypothesis.Test(context.Message, cancellationToken);
    }

    public static MessageHandler<TMessage> AsMessageHandler<TMessage, TMessageHandler>(
        this IHypothesis<TMessage> hypothesis, IServiceProvider serviceProvider)
        where TMessage : class, IMessage
        where TMessageHandler : IMessageHandler<TMessage>
    {
        return async (context, cancellationToken) =>
        {
            using var scope = serviceProvider.CreateScope();
            var bus = scope.ServiceProvider.GetRequiredService<IBus>();
            bus.Consume<TMessageHandler, TMessage>();

            var handler = scope.ServiceProvider.GetService(typeof(TMessageHandler));
            if (handler is null)
            {
                await hypothesis.Test(null!, cancellationToken);
                return;
            }

            await handler.InvokeMethodWithoutResultAsync("HandleAsync", context, cancellationToken);
            await hypothesis.Test(context.Message, cancellationToken);
        };
    }
}

public class MessageConsumer<T> : IMessageHandler<T>
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

    public async Task HandleAsync(IConsumeContext<T> messageContext, CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetService(_internalHandler);
        if (handler is null)
        {
            await _hypothesis.Test(null!, cancellationToken);
            return;
        }

        await handler.InvokeMethodAsync("HandleAsync", messageContext, cancellationToken);
        await _hypothesis.Test(messageContext.Message);
    }
}
