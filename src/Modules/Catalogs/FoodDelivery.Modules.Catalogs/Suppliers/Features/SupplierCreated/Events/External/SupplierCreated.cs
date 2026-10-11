using Squirrel.Abstractions.Messages;
using Squirrel.Core.Messages;

namespace FoodDelivery.Modules.Catalogs.Suppliers.Features.SupplierCreated.Events.External;

public record SupplierCreated(long Id, string Name) : IntegrationEvent;


public class SupplierCreatedConsumer : IMessageHandler<SupplierCreated>
{
    public Task Handle(SupplierCreated notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
