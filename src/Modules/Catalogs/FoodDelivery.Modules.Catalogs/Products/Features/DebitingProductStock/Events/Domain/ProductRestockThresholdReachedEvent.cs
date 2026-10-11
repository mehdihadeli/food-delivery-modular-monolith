using Ardalis.GuardClauses;
using Squirrel.Core.Events.Internal;
using Squirrel.Core.Events.Internal;
using FoodDelivery.Modules.Catalogs.Products.ValueObjects;
using FoodDelivery.Modules.Catalogs.Shared.Contracts;

namespace FoodDelivery.Modules.Catalogs.Products.Features.DebitingProductStock.Events.Domain;

public record ProductRestockThresholdReachedEvent(ProductId ProductId, Stock Stock, int Quantity) : DomainEvent;

internal class ProductRestockThresholdReachedEventHandler : IDomainEventHandler<ProductRestockThresholdReachedEvent>
{
    private readonly ICatalogDbContext _catalogDbContext;

    public ProductRestockThresholdReachedEventHandler(ICatalogDbContext catalogDbContext)
    {
        _catalogDbContext = catalogDbContext;
    }

    public ValueTask Handle(ProductRestockThresholdReachedEvent notification, CancellationToken cancellationToken)
    {
        Guard.Against.Null(notification, nameof(notification));

        // For example send an email to get more products
        return ValueTask.CompletedTask;
    }
}
