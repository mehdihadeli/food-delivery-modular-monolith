using BuildingBlocks.Abstractions.Scheduling;
using Hangfire;
using MediatR;

namespace BuildingBlocks.Integration.Hangfire;

// Ref: http://www.kamilgrzybek.com/design/processing-commands-with-hangfire-and-mediatr/
public class HangfireScheduler : IScheduler
{
    private readonly IScheduleExecutor _scheduleExecutor;

    public HangfireScheduler(IScheduleExecutor scheduleExecutor)
    {
        _scheduleExecutor = scheduleExecutor;
    }

    public void Enqueue<T>(T request)
        where T : class, IRequest
    {
        BackgroundJob.Enqueue(() => _scheduleExecutor.Execute(request));
    }

    public void Enqueue(object request)
    {
        BackgroundJob.Enqueue(() => _scheduleExecutor.Execute(request));
    }

    public void Schedule<T>(T request, DateTimeOffset scheduleAt)
        where T : class, IRequest
    {
        BackgroundJob.Schedule(() => _scheduleExecutor.Execute(request), scheduleAt);
    }

    public void Schedule(object request, DateTimeOffset scheduleAt)
    {
        BackgroundJob.Schedule(() => _scheduleExecutor.Execute(request), scheduleAt);
    }

    public void ScheduleRecurring<T>(T request, string cornExpression)
        where T : class, IRequest
    {
        RecurringJob.AddOrUpdate(
            typeof(T).Name,
            () => _scheduleExecutor.Execute(request),
            cornExpression,
            TimeZoneInfo.Local);
    }

    public void ScheduleRecurring(object request, string cornExpression)
    {
        RecurringJob.AddOrUpdate(
            request.GetType().Name,
            () => _scheduleExecutor.Execute(request),
            cornExpression,
            TimeZoneInfo.Local);
    }
}
