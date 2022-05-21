using BuildingBlocks.Abstractions.CQRS.Command;

namespace BuildingBlocks.Abstractions.Scheduling;

public interface ICommandScheduler
{
    Task ScheduleAsync(
        IInternalCommand internalCommandCommand,
        CancellationToken cancellationToken = default);

    Task ScheduleAsync(
        IInternalCommand[] internalCommandCommands,
        CancellationToken cancellationToken = default);
}
