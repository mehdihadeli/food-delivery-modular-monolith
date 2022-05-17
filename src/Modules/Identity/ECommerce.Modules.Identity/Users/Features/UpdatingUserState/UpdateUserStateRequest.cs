using ECommerce.Modules.Identity.Shared.Models;

namespace ECommerce.Modules.Identity.Users.Features.UpdatingUserState;

public record UpdateUserStateRequest
{
    public UserState UserState { get; init; }
}
