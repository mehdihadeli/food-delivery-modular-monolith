using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.Scheduling;
using MediatR;

namespace BuildingBlocks.Integration.Hangfire;

public class MediatRExecutor : IScheduleExecutor
{
    private readonly IMediator _mediator;

    public MediatRExecutor(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task Execute<T>(T request)
        where T : class, IRequest
    {
        Guard.Against.Null(request, nameof(request));

        return _mediator.Send(request);
    }

    public Task Execute(object request)
    {
        Guard.Against.Null(request, nameof(request));

        return _mediator.Send(request);
    }
}
