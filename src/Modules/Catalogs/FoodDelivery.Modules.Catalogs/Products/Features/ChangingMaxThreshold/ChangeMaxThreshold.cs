using Squirrel.Abstractions.Commands;

namespace FoodDelivery.Modules.Catalogs.Products.Features.ChangingMaxThreshold;

public record ChangeMaxThreshold(long ProductId, int NewMaxThreshold) : ITxCommand;
