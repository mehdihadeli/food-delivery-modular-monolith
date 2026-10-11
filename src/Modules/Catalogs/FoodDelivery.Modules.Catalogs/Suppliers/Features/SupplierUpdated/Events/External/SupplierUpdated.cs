using Squirrel.Abstractions.Messages;
using Squirrel.Core.Messages;

namespace FoodDelivery.Modules.Catalogs.Suppliers.Features.SupplierUpdated.Events.External;

public record SupplierUpdated(long Id, string Name) : IntegrationEvent;


public class SupplierUpdatedConsumer : IMessageHandler<SupplierUpdated>
{
    public Task Handle(SupplierUpdated notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
