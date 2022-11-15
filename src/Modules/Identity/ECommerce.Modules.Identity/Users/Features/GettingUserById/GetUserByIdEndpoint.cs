using Asp.Versioning.Conventions;
using BuildingBlocks.Abstractions.Web;
using ECommerce.Modules.Identity.Users.Features.RegisteringUser;

namespace ECommerce.Modules.Identity.Users.Features.GettingUserById;

public static class GetUserByIdEndpoint
{
    internal static IEndpointRouteBuilder MapGetUserByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet($"{UsersConfigs.UsersPrefixUri}/{{userId:guid}}", GetUserById)
            .AllowAnonymous()
            .WithTags(UsersConfigs.Tag)
            .Produces<RegisterUserResponse>(StatusCodes.Status200OK)
            .Produces<RegisterUserResponse>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("GetUserById")
            .WithDisplayName("Get User by Id.")
            .WithApiVersionSet(UsersConfigs.VersionSet)
            .HasApiVersion(1.0);

        return endpoints;
    }

    private static Task<IResult> GetUserById(
        Guid userId,
        IGatewayProcessor<IdentityModuleConfiguration> gatewayProcessor,
        CancellationToken cancellationToken)
    {
        return gatewayProcessor.ExecuteQuery(async queryProcessor =>
        {
            var result = await queryProcessor.SendAsync(new GetUserById(userId), cancellationToken);

            return Results.Ok(result);
        });
    }
}
