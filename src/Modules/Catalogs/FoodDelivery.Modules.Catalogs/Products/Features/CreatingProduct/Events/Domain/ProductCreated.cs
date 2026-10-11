using Ardalis.GuardClauses;
using Squirrel.Core.Events.Internal;
using Squirrel.Core.Events.Internal;
using Squirrel.Core.Exception;
using FoodDelivery.Modules.Catalogs.Products.Exceptions.Application;
using FoodDelivery.Modules.Catalogs.Products.Models;
using FoodDelivery.Modules.Catalogs.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Modules.Catalogs.Products.Features.CreatingProduct.Events.Domain;

public record ProductCreated(Product Product) : DomainEvent;

internal class ProductCreatedHandler : IDomainEventHandler<ProductCreated>
{
    private readonly CatalogDbContext _dbContext;

    public ProductCreatedHandler(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask Handle(ProductCreated notification, CancellationToken cancellationToken)
    {
        Guard.Against.Null(notification, nameof(notification));

        var existed = await _dbContext.ProductsView
            .SingleOrDefaultAsync(x => x.ProductId == notification.Product.Id.Value, cancellationToken);

        if (existed is null)
        {
            var product = await _dbContext.Products
                .Include(x => x.Brand)
                .Include(x => x.Category)
                .Include(x => x.Supplier)
                .SingleOrDefaultAsync(x => x.Id == notification.Product.Id.Value, cancellationToken);

            if (product is null)
                throw new ProductNotFoundException(notification.Product.Id);

            var productView = new ProductView
            {
                ProductId = product!.Id,
                ProductName = product.Name,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? string.Empty,
                SupplierId = product.SupplierId,
                SupplierName = product.Supplier?.Name ?? string.Empty,
                BrandId = product.BrandId,
                BrandName = product.Brand?.Name ?? string.Empty,
            };

            await _dbContext.Set<ProductView>().AddAsync(productView, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

// Mapping domain event to integration event in domain event handler is better from mapping in command handler (for preserving our domain rule invariants).
internal class ProductCreatedDomainEventToIntegrationMappingHandler : IDomainEventHandler<ProductCreated>
{
    public ProductCreatedDomainEventToIntegrationMappingHandler()
    {
    }

    public ValueTask Handle(ProductCreated domainEvent, CancellationToken cancellationToken)
    {
        // 1. Mapping DomainEvent To IntegrationEvent
        // 2. Save Integration Event to Outbox
        return ValueTask.CompletedTask;
    }
}
