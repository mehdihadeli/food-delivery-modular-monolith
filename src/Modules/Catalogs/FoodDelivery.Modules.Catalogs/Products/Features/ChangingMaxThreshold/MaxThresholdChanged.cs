using BuildingBlocks.Core.CQRS.Event.Internal;

namespace FoodDelivery.Modules.Catalogs.Products.Features.ChangingMaxThreshold;

public record MaxThresholdChanged(long ProductId, int MaxThreshold) : DomainEvent;
