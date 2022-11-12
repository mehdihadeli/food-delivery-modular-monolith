namespace ECommerce.Modules.Customers.Customers.Features.CreatingCustomer;

public record CreateCustomerResponse(
    long CustomerId,
    string Email,
    string FirstName,
    string LastName,
    Guid IdentityUserId);
