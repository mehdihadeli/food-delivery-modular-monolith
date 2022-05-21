namespace BuildingBlocks.Abstractions.Messaging;

public interface IBus : IBusProducer, IBusConsumer
{
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}
