using BuildingBlocks.Core.CQRS.Query;
using ECommerce.Modules.Customers.RestockSubscriptions.Dtos;

namespace ECommerce.Modules.Customers.RestockSubscriptions.Features.GettingRestockSubscriptions;

public record GetRestockSubscriptionsResponse(ListResultModel<RestockSubscriptionDto> RestockSubscriptions);
