using Squirrel.Core.Events.Internal;
using FoodDelivery.Modules.Catalogs.Brands;
using FoodDelivery.Modules.Catalogs.Products.ValueObjects;

namespace FoodDelivery.Modules.Catalogs.Products.Features.ChangingProductBrand.Events.Domain;

public record ProductBrandChanged(BrandId BrandId, ProductId ProductId) : DomainEvent;
