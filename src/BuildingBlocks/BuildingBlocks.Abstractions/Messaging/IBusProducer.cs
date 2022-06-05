namespace BuildingBlocks.Abstractions.Messaging;

public interface IBusProducer
{
    public Task PublishAsync<TMessage>(
        TMessage message,
        IDictionary<string, object?>? headers = null,
        CancellationToken cancellationToken = default)
        where TMessage : class, IMessage;

    public Task PublishAsync<TMessage>(
        TMessage message,
        IDictionary<string, object?>? headers = null,
        string? exchangeOrTopic = null,
        string? queue = null,
        CancellationToken cancellationToken = default)
        where TMessage : class, IMessage;

    public Task PublishAsync(
        object message,
        IDictionary<string, object?>? headers = null,
        CancellationToken cancellationToken = default);

    public Task PublishAsync(
        object message,
        IDictionary<string, object?>? headers = null,
        string? exchangeOrTopic = null,
        string? queue = null,
        CancellationToken cancellationToken = default);

    event Action<object> MessagePublished;
}
