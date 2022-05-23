using BuildingBlocks.Abstractions.Persistence;
using ECommerce.Modules.Identity.Identity.Data;
using ECommerce.Modules.Identity.Identity.Features.GetClaims;
using ECommerce.Modules.Identity.Identity.Features.Login;
using ECommerce.Modules.Identity.Identity.Features.Logout;
using ECommerce.Modules.Identity.Identity.Features.RefreshingToken;
using ECommerce.Modules.Identity.Identity.Features.RevokeRefreshToken;
using ECommerce.Modules.Identity.Identity.Features.SendEmailVerificationCode;
using ECommerce.Modules.Identity.Identity.Features.VerifyEmail;
using ECommerce.Modules.Identity.Shared.Extensions.ServiceCollectionExtensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace ECommerce.Modules.Identity.Identity;

internal static class IdentityConfigs
{
    public const string Tag = "Identity";
    public const string IdentityPrefixUri = $"{IdentityModuleConfiguration.IdentityModulePrefixUri}";

    internal static IServiceCollection AddIdentityServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IDataSeeder, IdentityDataSeeder>();
        services.AddCustomIdentity(configuration);
        services.AddCustomIdentityServer();

        return services;
    }

    internal static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
            $"{IdentityPrefixUri}/user-role",
            [Authorize(
                AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
                Roles = Constants.Role.User)]
            () => new {Role = Constants.Role.User}).WithTags("Identity");

        endpoints.MapGet(
            $"{IdentityPrefixUri}/admin-role",
            [Authorize(
                AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
                Roles = Constants.Role.Admin)]
            () => new {Role = Constants.Role.Admin}).WithTags("Identity");

        endpoints.MapLoginUserEndpoint();
        endpoints.MapLogoutEndpoint();
        endpoints.MapSendEmailVerificationCodeEndpoint();
        endpoints.MapSendVerifyEmailEndpoint();
        endpoints.MapRefreshTokenEndpoint();
        endpoints.MapRevokeTokenEndpoint();
        endpoints.MapGetClaimsEndpoint();

        return endpoints;
    }
}
