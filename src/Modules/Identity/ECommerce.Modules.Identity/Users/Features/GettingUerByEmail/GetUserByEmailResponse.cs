using ECommerce.Modules.Identity.Users.Dtos;

namespace ECommerce.Modules.Identity.Users.Features.GettingUerByEmail;

public record GetUserByEmailResponse(IdentityUserDto? UserIdentity);
