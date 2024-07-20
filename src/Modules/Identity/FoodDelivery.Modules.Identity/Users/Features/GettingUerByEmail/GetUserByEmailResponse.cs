using FoodDelivery.Modules.Identity.Users.Dtos;

namespace FoodDelivery.Modules.Identity.Users.Features.GettingUerByEmail;

public record GetUserByEmailResponse(IdentityUserDto? UserIdentity);
