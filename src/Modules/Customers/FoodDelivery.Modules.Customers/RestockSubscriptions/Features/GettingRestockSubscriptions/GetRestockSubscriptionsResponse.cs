using Squirrel.Abstractions.Core.Paging;
using FoodDelivery.Modules.Customers.RestockSubscriptions.Dtos;

namespace FoodDelivery.Modules.Customers.RestockSubscriptions.Features.GettingRestockSubscriptions;

public record GetRestockSubscriptionsResponse(IPageList<RestockSubscriptionDto> RestockSubscriptions);
