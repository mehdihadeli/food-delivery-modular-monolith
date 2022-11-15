using Asp.Versioning.Conventions;
using BuildingBlocks.Abstractions.Web;
using ECommerce.Modules.Identity.Users.Features.RegisteringUser;

namespace ECommerce.Modules.Identity.Users.Features.GettingUerByEmail;

// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing
// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis
public static class GetUserByEmailEndpoint
{
    internal static IEndpointRouteBuilder MapGetUserByEmailEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet($"{UsersConfigs.UsersPrefixUri}/by-email/{{email}}", GetUserByEmail)
            .AllowAnonymous()
            .WithTags(UsersConfigs.Tag)
            .Produces<RegisterUserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("GetUserByEmail")
            .WithDisplayName("Get User by email.")
            .WithApiVersionSet(UsersConfigs.VersionSet)
            .HasApiVersion(1.0);

        return endpoints;
    }

    private static Task<IResult> GetUserByEmail(
        [FromRoute] string email,
        IGatewayProcessor<IdentityModuleConfiguration> gatewayProcessor,
        CancellationToken cancellationToken)
    {
        return gatewayProcessor.ExecuteQuery(async queryProcessor =>
        {
            var result = await queryProcessor.SendAsync(new GetUserByEmail(email), cancellationToken);

            return Results.Ok(result);
        });
    }
}
