namespace FoodDelivery.Modules.Customers.RestockSubscriptions.Features.DeletingRestockSubscriptionsByTime;

public record DeleteRestockSubscriptionByTimeRequest(DateTime? From = null, DateTime? To = null);
