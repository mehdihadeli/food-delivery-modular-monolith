using BuildingBlocks.Core.Messaging;
using FoodDelivery.Modules.Identity.Shared.Models;

namespace FoodDelivery.Modules.Identity.Users.Features.UpdatingUserState.Events.Integration;

public record UserStateUpdated(Guid UserId, UserState OldUserState, UserState NewUserState) : IntegrationEvent;
