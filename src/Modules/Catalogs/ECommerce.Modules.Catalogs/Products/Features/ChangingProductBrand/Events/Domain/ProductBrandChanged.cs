using BuildingBlocks.Core.CQRS.Event.Internal;
using ECommerce.Modules.Catalogs.Brands;
using ECommerce.Modules.Catalogs.Products.ValueObjects;

namespace ECommerce.Modules.Catalogs.Products.Features.ChangingProductBrand.Events.Domain;

internal record ProductBrandChanged(BrandId BrandId, ProductId ProductId) : DomainEvent;
