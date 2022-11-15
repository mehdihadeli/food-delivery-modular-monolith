using Asp.Versioning.Conventions;
using BuildingBlocks.Abstractions.Web;

namespace ECommerce.Modules.Identity.Identity.Features.Login;

public static class LoginEndpoint
{
    internal static IEndpointRouteBuilder MapLoginUserEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"{IdentityConfigs.IdentityPrefixUri}/login", (
                LoginUserRequest request,
                IGatewayProcessor<IdentityModuleConfiguration> gatewayProcessor,
                CancellationToken cancellationToken) =>
            {
                return gatewayProcessor.ExecuteCommand(async commandProcessor =>
                {
                    var command = new Login(request.UserNameOrEmail, request.Password, request.Remember);

                    var result = await commandProcessor.SendAsync(command, cancellationToken);

                    return Results.Ok(result);
                });
            })
            .AllowAnonymous()
            .WithTags(IdentityConfigs.Tag)
            .Produces<LoginResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .WithDisplayName("Login User.")
            .WithApiVersionSet(IdentityConfigs.VersionSet)
            .HasApiVersion(1.0)
            .HasApiVersion(2.0);

        return endpoints;
    }
}
