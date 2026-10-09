using Squirrel.Core.Events.Internal;

namespace FoodDelivery.Modules.Catalogs.Products.Features.ChangingRestockThreshold;

public record RestockThresholdChanged(long ProductId, int RestockThreshold) : DomainEvent;
