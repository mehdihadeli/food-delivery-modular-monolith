using System.Threading.Channels;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.Context;
using BuildingBlocks.Abstractions.Serialization;
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
    private readonly IServiceProvider _serviceProvider;
    private readonly IMessageSerializer _messageSerializer;
    private readonly ILogger<InMemoryBus> _logger;
    private static readonly Channel<string> _channel;
    private static readonly Dictionary<Type, List<Type>> _handlers = new();
    private static readonly Dictionary<Type, Func<object, CancellationToken, Task>> _delegateHandlers = new();

    private static event Action<object, Type>? ConsumedMessage;
    private static event Action<object>? PublishedMessage;

    static InMemoryBus()
    {
        // We can use unbounded channel if we want to store unlimited message to channel.
        _channel = Channel.CreateBounded<string>(new BoundedChannelOptions(500)
        {
            AllowSynchronousContinuations = true,
            SingleReader = true,
            SingleWriter = true,
            FullMode = BoundedChannelFullMode.Wait
        });
    }

    public InMemoryBus(
        IServiceProvider serviceProvider,
        IMessageSerializer messageSerializer,
        ILogger<InMemoryBus> logger)
    {
        _serviceProvider = serviceProvider;
        _messageSerializer = messageSerializer;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        var threads = 5;

        Task.WhenAll(Enumerable.Range(0, threads)
            .Select(_ => Task.Factory.StartNew(
                () => ReceivingMessages(cancellationToken),
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default))
            .ToArray());

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    private async Task ReceivingMessages(CancellationToken cancellationToken)
    {
        // https://devblogs.microsoft.com/dotnet/an-introduction-to-system-threading-channels/
        await foreach (string messageEnvelopeJson in _channel.Reader.ReadAllAsync(cancellationToken))
        {
            MessageEnvelope? messageEnvelope =
                _messageSerializer.Deserialize<MessageEnvelope>(messageEnvelopeJson, true);

            if (messageEnvelope is null)
                continue;

            var messageTypeName = messageEnvelope.GetMessageType();

            try
            {
                foreach (var (messageType, handlersType) in
                         _handlers.Where(x => x.Key.Name == messageTypeName))
                {
                    var data = _messageSerializer.Deserialize(
                        messageEnvelope.Message.ToString()!,
                        messageType);

                    var ctx = new ConsumeContext(
                        data!,
                        messageEnvelope.Headers,
                        messageEnvelope.GetMessageId(),
                        TypeMapper.GetFullTypeName(messageType),
                        0,
                        0,
                        DateTime.Now);

                    var typedConsumedContext = typeof(ConsumeContextExtensions).InvokeGenericExtensionMethod(
                        nameof(ConsumeContextExtensions.ToTypedConsumeContext),
                        new[] {messageType},
                        null,
                        ctx);

                    foreach (var handlerType in handlersType)
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            object? handler = scope.ServiceProvider.GetService(handlerType);

                            if (handler is null)
                                return;

                            await ReflectionExtensions.InvokeMethodWithoutResultAsync(
                                handler,
                                nameof(IMessageHandler<IMessage>.HandleAsync),
                                typedConsumedContext,
                                cancellationToken);

                            ConsumedMessage?.Invoke(typedConsumedContext?.Message, handlerType);
                        }
                    }
                }

                foreach (var (messageType, delegateHandler) in
                         _delegateHandlers.Where(x => x.Key.Name == messageTypeName))
                {
                    var data = _messageSerializer.Deserialize(
                        messageEnvelope.Message.ToString()!,
                        messageType);

                    var ctx = new ConsumeContext(
                        data!,
                        messageEnvelope.Headers,
                        messageEnvelope.GetMessageId(),
                        TypeMapper.GetFullTypeName(messageType),
                        0,
                        0,
                        DateTime.Now);

                    var typedConsumedContext = typeof(ConsumeContextExtensions).InvokeGenericExtensionMethod(
                        nameof(ConsumeContextExtensions.ToTypedConsumeContext),
                        new[] {messageType},
                        null,
                        ctx);

                    await delegateHandler.Invoke(typedConsumedContext, cancellationToken);

                    ConsumedMessage?.Invoke(typedConsumedContext?.Message, delegateHandler.GetType());
                }
            }
            catch (System.Exception e)
            {
                _logger.LogError(e.Message);
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
        var messageEnvelope = new MessageEnvelope(message, metadata);

        var json = _messageSerializer.Serialize(messageEnvelope);

        await _channel.Writer.WriteAsync(json, cancellationToken);

        PublishedMessage?.Invoke(message);
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
        var messageEnvelope = new MessageEnvelope(message, metadata);

        var json = _messageSerializer.Serialize(messageEnvelope);

        await _channel.Writer.WriteAsync(json, cancellationToken);

        PublishedMessage?.Invoke(message);
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

    public void Consume<TMessage>(
        IMessageHandler<TMessage> handler,
        Action<IConsumeConfigurationBuilder>? consumeBuilder = null)
        where TMessage : class, IMessage
    {
        AddConsumerHandler(typeof(TMessage), handler.GetType());
    }

    public void Consume<TMessage>(
        MessageHandler<TMessage> subscribeMethod,
        Action<IConsumeConfigurationBuilder>? consumeBuilder = null)
        where TMessage : class, IMessage
    {
        Func<object, CancellationToken, Task> genericHandler =
            (context, ct) => subscribeMethod((IConsumeContext<TMessage>)context, ct);

        _delegateHandlers.Add(typeof(TMessage), genericHandler);
    }

    public void Consume<TMessage>()
        where TMessage : class, IMessage
    {
        Consume(typeof(TMessage));
    }

    public void Consume(Type messageType)
    {
        var handlerType = typeof(IMessageHandler<>).MakeGenericType(messageType);

        AddConsumerHandler(messageType, handlerType);
    }

    public void RemoveConsume(Type messageType)
    {
        var itemsToRemove = _handlers.Where(x => x.Key == messageType).ToDictionary(x => x.Key, v => v.Value);

        foreach (var toRemove in itemsToRemove)
        {
            _handlers.Remove(toRemove.Key);
        }
    }

    public void Consume<THandler, TMessage>()
        where THandler : IMessageHandler<TMessage>
        where TMessage : class, IMessage
    {
        Consume<TMessage>();
    }

    public void ConsumeAll()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            var messageTypes = assembly.GetHandledMessageTypes();

            foreach (var messageType in messageTypes)
            {
                Consume(messageType);
            }
        }
    }

    public void ConsumeAllFromAssemblyOf<TType>()
    {
        var messageTypes = typeof(TType).Assembly.GetHandledMessageTypes();

        foreach (var messageType in messageTypes)
        {
            Consume(messageType);
        }
    }

    public void ConsumeAllFromAssemblyOf(params Type[] assemblyMarkerTypes)
    {
        var assemblies = assemblyMarkerTypes.Select(x => x.Assembly).Distinct();

        foreach (var assembly in assemblies)
        {
            var messageTypes = assembly.GetHandledMessageTypes();

            foreach (var messageType in messageTypes)
            {
                Consume(messageType);
            }
        }
    }

    public void RemoveAllConsume()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            var messageTypes = assembly.GetHandledMessageTypes();

            foreach (var messageType in messageTypes)
            {
                RemoveConsume(messageType);
            }
        }
    }

    public void RemoveAllConsumeFromAssemblyOf<TType>()
    {
        var messageTypes = typeof(TType).Assembly.GetHandledMessageTypes();

        foreach (var messageType in messageTypes)
        {
            RemoveConsume(messageType);
        }
    }

    public void RemoveAllConsumeFromAssemblyOf(params Type[] assemblyMarkerTypes)
    {
        var assemblies = assemblyMarkerTypes.Select(x => x.Assembly).Distinct();

        foreach (var assembly in assemblies)
        {
            var messageTypes = assembly.GetHandledMessageTypes();

            foreach (var messageType in messageTypes)
            {
                RemoveConsume(messageType);
            }
        }
    }

    public event Action<object, Type>? MessageConsumed
    {
        add
        {
            ConsumedMessage += value;
        }
        remove
        {
            ConsumedMessage -= value;
        }
    }

    public event Action<object>? MessagePublished
    {
        add
        {
            PublishedMessage += value;
        }
        remove
        {
            PublishedMessage -= value;
        }
    }

    private static void AddConsumerHandler(Type messageType, Type handlerType)
    {
        if (!_handlers.TryGetValue(messageType, out var list))
        {
            list = new List<Type>();
            _handlers.Add(messageType, list);
        }

        list.Add(handlerType);
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
        meta.AddMessageType(message.GetType().Name);
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
        meta.AddMessageType(message.GetType().Name);
        meta.AddCreatedUnixTime(DateTime.Now.ToUnixTimeSecond());

        return meta;
    }
}
