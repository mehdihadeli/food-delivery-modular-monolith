using BuildingBlocks.Core.CQRS.Event.Internal;
using FoodDelivery.Modules.Catalogs.Brands;
using FoodDelivery.Modules.Catalogs.Products.ValueObjects;

namespace FoodDelivery.Modules.Catalogs.Products.Features.ChangingProductBrand.Events.Domain;

internal record ProductBrandChanged(BrandId BrandId, ProductId ProductId) : DomainEvent;
