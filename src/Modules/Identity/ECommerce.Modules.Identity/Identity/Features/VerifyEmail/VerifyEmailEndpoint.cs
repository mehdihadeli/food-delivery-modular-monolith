using Asp.Versioning.Conventions;
using BuildingBlocks.Abstractions.Web;

namespace ECommerce.Modules.Identity.Identity.Features.VerifyEmail;

public static class VerifyEmailEndpoint
{
    internal static IEndpointRouteBuilder MapSendVerifyEmailEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                $"{IdentityConfigs.IdentityPrefixUri}/verify-email", VerifyEmail)
            .WithTags(IdentityConfigs.Tag)
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("VerifyEmail")
            .WithDisplayName("Verify Email.")
            .WithApiVersionSet(IdentityConfigs.VersionSet)
            .HasApiVersion(1.0);

        return endpoints;
    }

    private static Task<IResult> VerifyEmail(
        VerifyEmailRequest request,
        IGatewayProcessor<IdentityModuleConfiguration> gatewayProcessor,
        CancellationToken cancellationToken)
    {
        return gatewayProcessor.ExecuteCommand(async commandProcessor =>
        {
            var command = new VerifyEmail(request.Email, request.Code);

            await commandProcessor.SendAsync(command, cancellationToken);

            return Results.Ok();
        });
    }
}
