using BuildingBlocks.Core.Messaging;

namespace FoodDelivery.Modules.Identity.Users.Features.RegisteringUser.Events.Integration;

public record UserRegistered(
    Guid IdentityId,
    string Email,
    string UserName,
    string FirstName,
    string LastName,
    List<string>? Roles) : IntegrationEvent;
