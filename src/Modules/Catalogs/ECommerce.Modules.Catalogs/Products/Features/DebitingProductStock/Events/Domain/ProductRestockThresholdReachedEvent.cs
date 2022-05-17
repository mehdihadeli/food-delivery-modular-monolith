using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Event.Internal;
using BuildingBlocks.Core.CQRS.Event.Internal;
using ECommerce.Modules.Catalogs.Products.ValueObjects;
using ECommerce.Modules.Catalogs.Shared.Contracts;

namespace ECommerce.Modules.Catalogs.Products.Features.DebitingProductStock.Events.Domain;

public record ProductRestockThresholdReachedEvent(ProductId ProductId, Stock Stock, int Quantity) : DomainEvent;

internal class ProductRestockThresholdReachedEventHandler : IDomainEventHandler<ProductRestockThresholdReachedEvent>
{
    private readonly ICatalogDbContext _catalogDbContext;

    public ProductRestockThresholdReachedEventHandler(ICatalogDbContext catalogDbContext)
    {
        _catalogDbContext = catalogDbContext;
    }

    public Task Handle(ProductRestockThresholdReachedEvent notification, CancellationToken cancellationToken)
    {
        Guard.Against.Null(notification, nameof(notification));

        // For example send an email to get more products
        return Task.CompletedTask;
    }
}
