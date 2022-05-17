using BuildingBlocks.Core.CQRS.Event.Internal;
using ECommerce.Modules.Catalogs.Brands;
using ECommerce.Modules.Catalogs.Categories;
using ECommerce.Modules.Catalogs.Products.Models;
using ECommerce.Modules.Catalogs.Products.ValueObjects;
using ECommerce.Modules.Catalogs.Suppliers;

namespace ECommerce.Modules.Catalogs.Products.Features.CreatingProduct.Events.Domain;

public record CreatingProduct(
    ProductId Id,
    Name Name,
    Price Price,
    Stock Stock,
    ProductStatus Status,
    Dimensions Dimensions,
    Category? Category,
    Supplier? Supplier,
    Brand? Brand,
    string? Description = null) : DomainEvent;
