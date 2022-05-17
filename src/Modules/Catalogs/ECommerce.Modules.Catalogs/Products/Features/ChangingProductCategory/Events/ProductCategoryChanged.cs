using BuildingBlocks.Core.CQRS.Event.Internal;
using ECommerce.Modules.Catalogs.Categories;
using ECommerce.Modules.Catalogs.Products.ValueObjects;

namespace ECommerce.Modules.Catalogs.Products.Features.ChangingProductCategory.Events;

public record ProductCategoryChanged(CategoryId CategoryId, ProductId ProductId) :
    DomainEvent;
