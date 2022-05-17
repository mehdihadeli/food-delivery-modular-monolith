using BuildingBlocks.Core.Messaging;
using ECommerce.Modules.Identity.Shared.Models;

namespace ECommerce.Modules.Identity.Users.Features.UpdatingUserState.Events.Integration;

public record UserStateUpdated(Guid UserId, UserState OldUserState, UserState NewUserState) : IntegrationEvent;
