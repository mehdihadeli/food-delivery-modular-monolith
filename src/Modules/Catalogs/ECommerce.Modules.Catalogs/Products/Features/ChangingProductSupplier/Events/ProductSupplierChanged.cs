using BuildingBlocks.Core.CQRS.Event.Internal;
using ECommerce.Modules.Catalogs.Products.ValueObjects;
using ECommerce.Modules.Catalogs.Suppliers;

namespace ECommerce.Modules.Catalogs.Products.Features.ChangingProductSupplier.Events;

public record ProductSupplierChanged(SupplierId SupplierId, ProductId ProductId) : DomainEvent;
