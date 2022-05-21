using BuildingBlocks.Abstractions.CQRS.Command;
using BuildingBlocks.Abstractions.Scheduling;

namespace BuildingBlocks.Integration.Hangfire;

public class HangfireCommandScheduler : ICommandScheduler
{
    private readonly IScheduler _scheduler;

    public HangfireCommandScheduler(IScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    public Task ScheduleAsync(IInternalCommand internalCommandCommand, CancellationToken cancellationToken = default)
    {
        _scheduler.Enqueue(internalCommandCommand);

        return Task.CompletedTask;
    }

    public async Task ScheduleAsync(
        IInternalCommand[] internalCommandCommands,
        CancellationToken cancellationToken = default)
    {
        foreach (var internalCommandCommand in internalCommandCommands)
        {
            await ScheduleAsync(internalCommandCommand, cancellationToken);
        }
    }
}
