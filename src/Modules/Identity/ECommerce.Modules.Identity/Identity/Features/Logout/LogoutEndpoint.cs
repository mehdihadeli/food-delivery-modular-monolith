using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Authentication;

namespace ECommerce.Modules.Identity.Identity.Features.Logout;

public static class LogoutEndpoint
{
    internal static IEndpointRouteBuilder MapLogoutEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"{IdentityConfigs.IdentityPrefixUri}/logout", async (HttpContext httpContext) =>
            {
                await httpContext.SignOutAsync();
                return Results.Ok();
            })
            .Produces(StatusCodes.Status200OK)
            .WithTags(IdentityConfigs.Tag)
            .RequireAuthorization()
            .WithName("logout")
            .WithDisplayName("Logout User.")
            .WithApiVersionSet(IdentityConfigs.VersionSet)
            .HasApiVersion(1.0);

        return endpoints;
    }
}
