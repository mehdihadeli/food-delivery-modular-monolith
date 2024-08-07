using Ardalis.GuardClauses;
using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Command;
using BuildingBlocks.Core.CQRS.Command;
using FoodDelivery.Modules.Customers.RestockSubscriptions.Models.Read;
using FoodDelivery.Modules.Customers.Shared.Data;

namespace FoodDelivery.Modules.Customers.RestockSubscriptions.Features;

public record CreateMongoRestockSubscriptionReadModels(
    long RestockSubscriptionId,
    long CustomerId,
    string CustomerName,
    long ProductId,
    string ProductName,
    string Email,
    DateTime Created,
    bool Processed,
    DateTime? ProcessedTime = null) : InternalCommand
{
    public bool IsDeleted { get; init; }
}

internal class CreateRestockSubscriptionReadModelHandler : ICommandHandler<CreateMongoRestockSubscriptionReadModels>
{
    private readonly CustomersReadDbContext _mongoDbContext;
    private readonly IMapper _mapper;

    public CreateRestockSubscriptionReadModelHandler(CustomersReadDbContext mongoDbContext, IMapper mapper)
    {
        _mongoDbContext = mongoDbContext;
        _mapper = mapper;
    }

    public async Task<Unit> Handle(
        CreateMongoRestockSubscriptionReadModels command,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(command, nameof(command));

        var readModel = _mapper.Map<RestockSubscriptionReadModel>(command);

        await _mongoDbContext.RestockSubscriptions.InsertOneAsync(readModel, cancellationToken: cancellationToken);

        return Unit.Value;
    }
}
