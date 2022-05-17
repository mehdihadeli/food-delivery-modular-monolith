namespace ECommerce.Modules.Customers.RestockSubscriptions.Features.CreatingRestockSubscription;

public record CreateRestockSubscriptionRequest(long CustomerId, long ProductId, string Email);
