using ECommerce.Modules.Shared.Catalogs.Products.Events.Integration;
using MassTransit;

namespace ECommerce.Modules.Customers.Products.Features.CreatingProduct.Events.External;

public class ProductCreatedConsumer : IConsumer<ProductCreated>
{
    public Task Consume(ConsumeContext<ProductCreated> context)
    {
        return Task.CompletedTask;
    }
}
