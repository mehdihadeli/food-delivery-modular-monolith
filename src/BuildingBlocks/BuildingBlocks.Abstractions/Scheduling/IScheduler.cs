using MediatR;

namespace BuildingBlocks.Abstractions.Scheduling;

public interface IScheduler
{
    void Enqueue<T>(T request)
        where T : class, IRequest;

    void Enqueue(object request);

    void Schedule<T>(T request, DateTimeOffset scheduleAt)
        where T : class, IRequest;

    void Schedule(object request, DateTimeOffset scheduleAt);

    void ScheduleRecurring<T>(T request, string cornExpression)
        where T : class, IRequest;

    void ScheduleRecurring(object request, string cornExpression);
}
