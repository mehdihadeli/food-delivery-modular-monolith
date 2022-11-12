using BuildingBlocks.Core.CQRS.Query;
using ECommerce.Modules.Identity.Users.Dtos;

namespace ECommerce.Modules.Identity.Users.Features.GettingUsers;

public record GetUsersResponse(ListResultModel<IdentityUserDto> IdentityUsers);
