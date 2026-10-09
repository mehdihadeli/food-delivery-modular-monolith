using Squirrel.Core.Events.Internal;

namespace FoodDelivery.Modules.Catalogs.Products.Features.ChangingMaxThreshold;

public record MaxThresholdChanged(long ProductId, int MaxThreshold) : DomainEvent;
