using Squirrel.Core.Queries;
using FoodDelivery.Modules.Customers.RestockSubscriptions.Dtos;

namespace FoodDelivery.Modules.Customers.RestockSubscriptions.Features.GettingRestockSubscriptions;

public record GetRestockSubscriptionsResponse(ListResultModel<RestockSubscriptionDto> RestockSubscriptions);
