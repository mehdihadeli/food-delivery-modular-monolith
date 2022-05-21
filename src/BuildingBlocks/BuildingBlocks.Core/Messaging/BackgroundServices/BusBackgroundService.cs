using BuildingBlocks.Abstractions.Messaging;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Core.Messaging.BackgroundServices;

public class BusBackgroundService : BackgroundService
{
    private readonly IBus _bus;

    public BusBackgroundService(IBus bus)
    {
        _bus = bus;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return _bus.StartAsync(stoppingToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return _bus.StopAsync(cancellationToken);
    }
}
