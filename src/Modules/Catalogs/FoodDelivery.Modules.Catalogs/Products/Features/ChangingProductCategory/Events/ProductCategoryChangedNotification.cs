using BuildingBlocks.Core.CQRS.Event.Internal;
using FoodDelivery.Modules.Catalogs.Categories;
using FoodDelivery.Modules.Catalogs.Products.ValueObjects;

namespace FoodDelivery.Modules.Catalogs.Products.Features.ChangingProductCategory.Events;

public record ProductCategoryChangedNotification(CategoryId CategoryId, ProductId ProductId) : DomainEvent;
