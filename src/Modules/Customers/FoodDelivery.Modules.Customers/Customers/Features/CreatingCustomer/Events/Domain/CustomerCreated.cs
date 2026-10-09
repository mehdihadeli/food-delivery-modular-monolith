using Ardalis.GuardClauses;
using AutoMapper;
using Squirrel.Abstractions.Commands;
using Squirrel.Core.Events.Internal;
using FoodDelivery.Modules.Customers.Customers.Models;

namespace FoodDelivery.Modules.Customers.Customers.Features.CreatingCustomer.Events.Domain;

public record CustomerCreated(Customer Customer) : DomainEvent;

internal class CustomerCreatedHandler : IDomainEventHandler<CustomerCreated>
{
    private readonly ICommandBus _commandBus;
    private readonly IMapper _mapper;

    public CustomerCreatedHandler(ICommandBus commandBus, IMapper mapper)
    {
        _commandBus = commandBus;
        _mapper = mapper;
    }

    public Task Handle(CustomerCreated notification, CancellationToken cancellationToken)
    {
        Guard.Against.Null(notification, nameof(notification));

        var mongoReadCommand = _mapper.Map<CreateMongoCustomerReadModels>(notification.Customer);

        // https://github.com/kgrzybek/modular-monolith-with-ddd#38-internal-processing
        // Schedule multiple read sides to execute here
        return _commandBus.ScheduleAsync(new IInternalCommand[] { mongoReadCommand }, cancellationToken);
    }
}
