using BuildingBlocks.Abstractions.CQRS.Command;
using BuildingBlocks.Abstractions.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

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
            .WithDisplayName("Refresh Token.");

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
