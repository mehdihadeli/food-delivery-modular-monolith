using BuildingBlocks.Core.CQRS.Event.Internal;
using FoodDelivery.Modules.Catalogs.Products.ValueObjects;
using FoodDelivery.Modules.Catalogs.Suppliers;

namespace FoodDelivery.Modules.Catalogs.Products.Features.ChangingProductSupplier.Events;

public record ProductSupplierChanged(SupplierId SupplierId, ProductId ProductId) : DomainEvent;
