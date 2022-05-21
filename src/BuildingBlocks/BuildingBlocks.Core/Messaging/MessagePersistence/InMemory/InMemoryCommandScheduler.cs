using BuildingBlocks.Abstractions.CQRS.Command;
using BuildingBlocks.Abstractions.Messaging.PersistMessage;
using BuildingBlocks.Abstractions.Scheduling;

namespace BuildingBlocks.Core.Messaging.MessagePersistence.InMemory;

public class InMemoryCommandScheduler : ICommandScheduler
{
    private readonly IMessagePersistenceService _messagePersistenceService;

    public InMemoryCommandScheduler(IMessagePersistenceService messagePersistenceService)
    {
        _messagePersistenceService = messagePersistenceService;
    }

    public async Task ScheduleAsync(
        IInternalCommand internalCommandCommand,
        CancellationToken cancellationToken = default)
    {
        await _messagePersistenceService.AddInternalMessageAsync(internalCommandCommand, cancellationToken);
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
