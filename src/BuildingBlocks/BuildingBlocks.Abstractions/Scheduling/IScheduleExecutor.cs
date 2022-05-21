using MediatR;

namespace BuildingBlocks.Abstractions.Scheduling;

public interface IScheduleExecutor
{
    Task Execute<T>(T request)
        where T : class, IRequest;

    Task Execute(object request);
}
