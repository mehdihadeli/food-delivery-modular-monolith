using Squirrel.Abstractions.Messages;
using Squirrel.Core.Messages;

namespace FoodDelivery.Modules.Catalogs.Suppliers.Features.SupplierDeleted.Events.External;

public record SupplierDeleted(long Id) : IntegrationEvent;

public class SupplierDeletedConsumer : IMessageHandler<SupplierDeleted>
{
    public Task Handle(SupplierDeleted notification, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
