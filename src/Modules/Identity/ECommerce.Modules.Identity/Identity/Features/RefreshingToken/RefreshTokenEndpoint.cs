using Asp.Versioning.Conventions;
using BuildingBlocks.Abstractions.Web;

namespace ECommerce.Modules.Identity.Identity.Features.RefreshingToken;

public static class RefreshTokenEndpoint
{
    internal static IEndpointRouteBuilder MapRefreshTokenEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"{IdentityConfigs.IdentityPrefixUri}/refresh-token", RefreshToken)
            .WithTags(IdentityConfigs.Tag)
            .RequireAuthorization()
            .Produces<RefreshTokenResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("RefreshToken")
            .WithDisplayName("Refresh Token.")
            .WithApiVersionSet(IdentityConfigs.VersionSet)
            .HasApiVersion(1.0);

        return endpoints;
    }

    private static Task<IResult> RefreshToken(
        RefreshTokenRequest request,
        IGatewayProcessor<IdentityModuleConfiguration> gatewayProcessor,
        CancellationToken cancellationToken)
    {
        return gatewayProcessor.ExecuteCommand(async commandProcessor =>
        {
            var command = new RefreshToken(request.AccessToken, request.RefreshToken);

            var result = await commandProcessor.SendAsync(command, cancellationToken);

            return Results.Ok(result);
        });
    }
}
