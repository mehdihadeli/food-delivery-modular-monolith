using Squirrel.Core.Events.Internal;
using FoodDelivery.Modules.Catalogs.Products.ValueObjects;

namespace FoodDelivery.Modules.Catalogs.Products.Features.ChangingProductPrice;

public record ProductPriceChanged(Price Price) : DomainEvent;
