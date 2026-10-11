using Squirrel.Abstractions.Core.Paging;
using FoodDelivery.Modules.Identity.Users.Dtos;

namespace FoodDelivery.Modules.Identity.Users.Features.GettingUsers;

public record GetUsersResponse(IPageList<IdentityUserDto> IdentityUsers);
