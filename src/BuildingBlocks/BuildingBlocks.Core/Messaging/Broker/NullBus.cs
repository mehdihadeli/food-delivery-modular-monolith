using BuildingBlocks.Abstractions.Messaging;

namespace BuildingBlocks.Core.Messaging.Broker;

public class NullBus : IBus
{
    public Task PublishAsync<TMessage>(
        TMessage message,
        IDictionary<string, object?>? headers,
        CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
    {
        return Task.CompletedTask;
    }

    public Task PublishAsync<TMessage>(
        TMessage message,
        IDictionary<string, object?>? headers,
        string? exchangeOrTopic = null,
        string? queue = null,
        CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
    {
        return Task.CompletedTask;
    }

    public Task PublishAsync(
        object message,
        IDictionary<string, object?>? headers,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task PublishAsync(
        object message,
        IDictionary<string, object?>? headers,
        string? exchangeOrTopic = null,
        string? queue = null,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public event Action<object>? MessagePublished;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public void Consume<TMessage>(
        IMessageHandler<TMessage> handler,
        Action<IConsumeConfigurationBuilder>? consumeBuilder = null)
        where TMessage : class, IMessage
    {
    }

    public void Consume<TMessage>(
        MessageHandler<TMessage> subscribeMethod,
        Action<IConsumeConfigurationBuilder>? consumeBuilder = null)
        where TMessage : class, IMessage
    {
    }

    public void Consume<TMessage>()
        where TMessage : class, IMessage
    {
    }

    public void Consume(Type messageType)
    {
    }

    public void Consume<THandler, TMessage>()
        where THandler : IMessageHandler<TMessage>
        where TMessage : class, IMessage
    {
    }

    public void ConsumeAll()
    {
    }

    public void ConsumeAllFromAssemblyOf<TType>()
    {
    }

    public void ConsumeAllFromAssemblyOf(params Type[] assemblyMarkerTypes)
    {
    }

    public void RemoveConsume(Type messageType)
    {
    }

    public void RemoveAllConsume()
    {
    }

    public void RemoveAllConsumeFromAssemblyOf<TType>()
    {
    }

    public void RemoveAllConsumeFromAssemblyOf(params Type[] assemblyMarkerTypes)
    {
    }

    public event Action<object, Type>? MessageConsumed;
}
