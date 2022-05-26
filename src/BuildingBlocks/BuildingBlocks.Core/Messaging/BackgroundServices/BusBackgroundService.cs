using BuildingBlocks.Abstractions.Messaging;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Core.Messaging.BackgroundServices;

public class BusBackgroundService : IHostedService
{
    private readonly IBus _bus;

    public BusBackgroundService(IBus bus)
    {
        _bus = bus;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _bus.StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _bus.StartAsync(cancellationToken);
    }
}
