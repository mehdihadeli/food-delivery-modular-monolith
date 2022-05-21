using System.Threading.Channels;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.Context;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Messaging.Context;
using BuildingBlocks.Core.Messaging.Extensions;
using BuildingBlocks.Core.Types;
using Humanizer;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Core.Messaging.Broker.InMemory;

// Ref: https://devblogs.microsoft.com/dotnet/an-introduction-to-system-threading-channels/
// https://michaelscodingspot.com/c-job-queues-with-reactive-extensions-and-channels/
// https://sourceexample.com/implement-asynchronous-friendly-producer-consumer-(pub-sub)-patterns-in-system.threading.channels-99360/
// https://deniskyashif.com/2019/12/08/csharp-channels-part-1/
public class InMemoryBus : IBus
{
    private readonly ILogger<InMemoryBus> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Channel<MessageEnvelope> _channel;
    private readonly Dictionary<Type, object> _handlers = new();
    private readonly Dictionary<Type, Func<object, CancellationToken, Task>> _delegateHandlers = new();

    public InMemoryBus(ILogger<InMemoryBus> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;

        // We can use unbounded channel if we want to store unlimited message to channel.
        _channel = Channel.CreateBounded<MessageEnvelope>(new BoundedChannelOptions(500)
        {
            AllowSynchronousContinuations = true,
            SingleReader = true,
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.Wait
        });
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
#pragma warning disable VSTHRD105
#pragma warning disable VSTHRD110

        // create 10 separate thread for consuming
        for (int i = 0; i < 10; i++)
        {
            Task.Factory.StartNew(
                async () =>
                {
                    await ReceivingMessages(cancellationToken);
                },
                TaskCreationOptions.LongRunning);
        }

#pragma warning restore VSTHRD110
#pragma warning restore VSTHRD105

        return Task.CompletedTask;
    }


    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        _channel.Writer.Complete();

        return Task.CompletedTask;
    }

    private async Task ReceivingMessages(CancellationToken cancellationToken)
    {
        // https://devblogs.microsoft.com/dotnet/an-introduction-to-system-threading-channels/
        await foreach (MessageEnvelope messageEnvelope in _channel.Reader.ReadAllAsync(cancellationToken))
        {
            bool handlerExists =
                _handlers.TryGetValue(messageEnvelope.Message.GetType(), out object? handler);

            bool delegateHandlerExists = _delegateHandlers.TryGetValue(
                messageEnvelope.Message.GetType(),
                out Func<object, CancellationToken, Task>? delegateHandler);

            var ctx = new ConsumeContext(
                messageEnvelope.Message,
                messageEnvelope.Headers,
                messageEnvelope.GetMessageId(),
                TypeMapper.GetTypeName(messageEnvelope.Message.GetType()),
                0,
                0,
                DateTime.Now);

            var messageType = messageEnvelope.Message.GetType();

            var typedConsumedContext = typeof(ConsumeContextExtensions).InvokeGenericExtensionMethod(
                nameof(ConsumeContextExtensions.ToTypedConsumeContext),
                new[] {messageType},
                null,
                ctx);

            if (delegateHandlerExists && delegateHandler is { })
            {
                await delegateHandler.Invoke(typedConsumedContext, cancellationToken);
            }

            if (handlerExists && handler is { })
            {
                ReflectionExtensions.InvokeMethodAsync(
                    handler,
                    nameof(IMessageHandler<IMessage>.HandleAsync),
                    typedConsumedContext,
                    cancellationToken);
            }
        }
    }

    public async Task PublishAsync<TMessage>(
        TMessage message,
        IDictionary<string, object?>? headers,
        CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
    {
        var metadata = GetMetadata(message, headers);

        await _channel.Writer.WriteAsync(new MessageEnvelope(message, metadata), cancellationToken);
    }

    public async Task PublishAsync<TMessage>(
        TMessage message,
        IDictionary<string, object?>? headers,
        string? exchangeOrTopic = null,
        string? queue = null,
        CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
    {
        await PublishAsync(message, headers, cancellationToken);
    }

    public async Task PublishAsync(
        object message,
        IDictionary<string, object?>? headers,
        CancellationToken cancellationToken = default)
    {
        var metadata = GetMetadata(message, headers);

        await _channel.Writer.WriteAsync(new MessageEnvelope(message, metadata), cancellationToken);
    }

    public async Task PublishAsync(
        object message,
        IDictionary<string, object?>? headers,
        string? exchangeOrTopic = null,
        string? queue = null,
        CancellationToken cancellationToken = default)
    {
        await PublishAsync(message, headers, cancellationToken);
    }

    public Task Consume<TMessage>(
        IMessageHandler<TMessage> handler,
        Action<IConsumeConfigurationBuilder>? consumeBuilder = null,
        CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
    {
        _handlers.Add(typeof(TMessage), handler);

        return Task.CompletedTask;
    }

    public Task Consume<TMessage>(
        MessageHandler<TMessage> subscribeMethod,
        Action<IConsumeConfigurationBuilder>? consumeBuilder = null,
        CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
    {
        Func<object, CancellationToken, Task> genericHandler =
            (context, ct) => subscribeMethod((IConsumeContext<TMessage>)context, ct);

        _delegateHandlers.Add(typeof(TMessage), genericHandler);

        return Task.CompletedTask;
    }

    public Task Consume<TMessage>(CancellationToken cancellationToken = default)
        where TMessage : class, IMessage
    {
        var handlerType = typeof(IMessageHandler<>).MakeGenericType(typeof(TMessage));

        using var scope = _scopeFactory.CreateScope();
        object handler = scope.ServiceProvider.GetService(handlerType);

        if (handler is null)
            return Task.CompletedTask;

        _handlers.AddOrReplace(typeof(TMessage), handler);

        return Task.CompletedTask;
    }

    public Task Consume(Type messageType, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IMessageHandler<>).MakeGenericType(messageType);

        _handlers.AddOrReplace(messageType, handlerType);

        return Task.CompletedTask;
    }

    public Task Consume<THandler, TMessage>(CancellationToken cancellationToken = default)
        where THandler : IMessageHandler<TMessage>
        where TMessage : class, IMessage
    {
        _handlers.AddOrReplace(typeof(TMessage), typeof(THandler));

        return Task.CompletedTask;
    }

    public Task ConsumeAll(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task ConsumeAllFromAssemblyOf<TType>(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task ConsumeAllFromAssemblyOf(
        CancellationToken cancellationToken = default,
        params Type[] assemblyMarkerTypes)
    {
        return Task.CompletedTask;
    }

    private static IDictionary<string, object?> GetMetadata<TMessage>(
        TMessage message,
        IDictionary<string, object?>? headers)
        where TMessage : class, IMessage
    {
        var meta = headers ?? new Dictionary<string, object?>();

        if (!meta.ContainsKey(MessageHeaders.MessageId))
        {
            var messageId = message.MessageId;
            meta.AddMessageId(messageId.ToString());
        }

        if (!meta.ContainsKey(MessageHeaders.CorrelationId))
        {
            meta.AddCorrelationId(Guid.NewGuid().ToString());
        }

        meta.AddMessageName(message.GetType().Name.Underscore());
        meta.AddMessageType(TypeMapper.GetTypeName(message.GetType()));
        meta.AddCreatedUnixTime(DateTime.Now.ToUnixTimeSecond());
        return meta;
    }

    private static IDictionary<string, object?> GetMetadata(object message, IDictionary<string, object?>? headers)
    {
        var meta = headers ?? new Dictionary<string, object?>();

        if (!meta.ContainsKey(MessageHeaders.MessageId))
        {
            var messageId = Guid.NewGuid();
            meta.AddMessageId(messageId.ToString());
        }

        if (!meta.ContainsKey(MessageHeaders.CorrelationId))
        {
            meta.AddCorrelationId(Guid.NewGuid().ToString());
        }

        meta.AddMessageName(message.GetType().Name.Underscore());
        meta.AddMessageType(TypeMapper.GetTypeName(message.GetType()));
        meta.AddCreatedUnixTime(DateTime.Now.ToUnixTimeSecond());
        return meta;
    }
}
