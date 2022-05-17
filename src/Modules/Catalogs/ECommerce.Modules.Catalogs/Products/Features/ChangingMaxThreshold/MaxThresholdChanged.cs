using BuildingBlocks.Core.CQRS.Event.Internal;

namespace ECommerce.Modules.Catalogs.Products.Features.ChangingMaxThreshold;

public record MaxThresholdChanged(long ProductId, int MaxThreshold) : DomainEvent;
