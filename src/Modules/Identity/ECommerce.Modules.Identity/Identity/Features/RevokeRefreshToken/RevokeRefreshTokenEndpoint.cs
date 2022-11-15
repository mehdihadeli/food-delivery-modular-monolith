using Asp.Versioning.Conventions;
using BuildingBlocks.Abstractions.Web;

namespace ECommerce.Modules.Identity.Identity.Features.RevokeRefreshToken;

public static class RevokeRefreshTokenEndpoint
{
    internal static IEndpointRouteBuilder MapRevokeTokenEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"{IdentityConfigs.IdentityPrefixUri}/revoke-refresh-token", RevokeToken)
            .WithTags(IdentityConfigs.Tag)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .WithDisplayName("Revoke refresh token.")
            .WithName("RevokeRefreshToken")
            .WithApiVersionSet(IdentityConfigs.VersionSet)
            .HasApiVersion(1.0);

        return endpoints;
    }

    private static Task<IResult> RevokeToken(
        RevokeRefreshTokenRequest request,
        IGatewayProcessor<IdentityModuleConfiguration> gatewayProcessor,
        CancellationToken cancellationToken)
    {
       return gatewayProcessor.ExecuteCommand(async commandProcessor =>
        {
            var command = new RevokeRefreshToken(request.RefreshToken);

            await commandProcessor.SendAsync(command, cancellationToken);

            return Results.NoContent();
        });
    }
}
