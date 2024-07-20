using FoodDelivery.Modules.Identity.Shared.Models;

namespace FoodDelivery.Modules.Identity.Users.Features.UpdatingUserState;

public record UpdateUserStateRequest
{
    public UserState UserState { get; init; }
}
