using BuildingBlocks.Core.CQRS.Event.Internal;
using FoodDelivery.Modules.Catalogs.Brands;
using FoodDelivery.Modules.Catalogs.Categories;
using FoodDelivery.Modules.Catalogs.Products.Models;
using FoodDelivery.Modules.Catalogs.Products.ValueObjects;
using FoodDelivery.Modules.Catalogs.Suppliers;

namespace FoodDelivery.Modules.Catalogs.Products.Features.CreatingProduct.Events.Domain;

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
