using BuildingBlocks.Abstractions.CQRS.Command;

namespace ECommerce.Modules.Catalogs.Products.Features.ChangingRestockThreshold;

public record ChangeRestockThreshold(long ProductId, int NewRestockThreshold) : ITxCommand;
