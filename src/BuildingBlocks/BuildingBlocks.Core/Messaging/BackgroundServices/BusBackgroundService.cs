using BuildingBlocks.Abstractions.Messaging;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Core.Messaging.BackgroundServices;

// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services
public class BusBackgroundService : BackgroundService
{
    private readonly IBus _bus;
    private Task? _executingTask;

    public BusBackgroundService(IBus bus)
    {
        _bus = bus;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _executingTask = _bus.StartAsync(stoppingToken);

        return _executingTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _bus.StopAsync(cancellationToken);

        await base.StopAsync(cancellationToken);
    }
}
