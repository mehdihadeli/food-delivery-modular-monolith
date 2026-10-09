using Squirrel.Core.Queries;
using FoodDelivery.Modules.Identity.Users.Dtos;

namespace FoodDelivery.Modules.Identity.Users.Features.GettingUsers;

public record GetUsersResponse(ListResultModel<IdentityUserDto> IdentityUsers);
