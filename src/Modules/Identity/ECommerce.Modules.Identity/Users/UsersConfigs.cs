using ECommerce.Modules.Identity.Users.Features.GettingUerByEmail;
using ECommerce.Modules.Identity.Users.Features.GettingUserById;
using ECommerce.Modules.Identity.Users.Features.RegisteringUser;
using ECommerce.Modules.Identity.Users.Features.UpdatingUserState;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Modules.Identity.Users;

internal static class UsersConfigs
{
    public const string Tag = "Users";
    public const string UsersPrefixUri = $"{IdentityModuleConfiguration.IdentityModulePrefixUri}/users";

    internal static IServiceCollection AddUsersServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services;
    }

    internal static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapRegisterNewUserEndpoint();
        endpoints.MapUpdateUserStateEndpoint();
        endpoints.MapGetUserByIdEndpoint();
        endpoints.MapGetUserByEmailEndpoint();

        return endpoints;
    }
}
