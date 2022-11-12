namespace ECommerce.Modules.Customers.Customers.Features.CreatingCustomer;

public record CreateCustomerResult(
    long CustomerId,
    string Email,
    string FirstName,
    string LastName,
    Guid IdentityUserId);
