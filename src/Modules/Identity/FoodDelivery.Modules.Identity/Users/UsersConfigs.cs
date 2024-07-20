using Asp.Versioning.Builder;
using FoodDelivery.Modules.Identity.Users.Features.GettingUerByEmail;
using FoodDelivery.Modules.Identity.Users.Features.GettingUserById;
using FoodDelivery.Modules.Identity.Users.Features.RegisteringUser;
using FoodDelivery.Modules.Identity.Users.Features.UpdatingUserState;

namespace FoodDelivery.Modules.Identity.Users;

internal static class UsersConfigs
{
    public const string Tag = "Users";
    public const string UsersPrefixUri = $"{IdentityModuleConfiguration.IdentityModulePrefixUri}/users";
    public static ApiVersionSet VersionSet { get; private set; } = default!;

    internal static IServiceCollection AddUsersServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services;
    }

    internal static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        VersionSet = endpoints.NewApiVersionSet(Tag).Build();

        endpoints.MapRegisterNewUserEndpoint();
        endpoints.MapUpdateUserStateEndpoint();
        endpoints.MapGetUserByIdEndpoint();
        endpoints.MapGetUserByEmailEndpoint();

        return endpoints;
    }
}
