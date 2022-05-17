using AutoMapper;
using ECommerce.Modules.Identity.Shared.Models;
using ECommerce.Modules.Identity.Users.Dtos;

namespace ECommerce.Modules.Identity.Users;

public class UsersMapping : Profile
{
    public UsersMapping()
    {
        CreateMap<ApplicationUser, IdentityUserDto>()
            .ForMember(x => x.RefreshTokens, opt => opt.MapFrom(x => x.RefreshTokens.Select(r => r.Token)));
    }
}
