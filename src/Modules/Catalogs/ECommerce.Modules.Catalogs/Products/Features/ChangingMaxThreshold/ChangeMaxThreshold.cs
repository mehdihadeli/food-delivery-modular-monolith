using BuildingBlocks.Abstractions.CQRS.Command;

namespace ECommerce.Modules.Catalogs.Products.Features.ChangingMaxThreshold;

public record ChangeMaxThreshold(long ProductId, int NewMaxThreshold) : ITxCommand;
