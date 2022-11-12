using BuildingBlocks.Abstractions.Web;
using ECommerce.Modules.Identity.Users.Features.RegisteringUser;

namespace ECommerce.Modules.Identity.Users.Features.UpdatingUserState;

public static class UpdateUserStateEndpoint
{
    internal static IEndpointRouteBuilder MapUpdateUserStateEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut($"{UsersConfigs.UsersPrefixUri}/{{userId:guid}}/state", UpdateUserState)
            .AllowAnonymous()
            .WithTags(UsersConfigs.Tag)
            .Produces<RegisterUserResponse>(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .WithDisplayName("Update User State.");

        return endpoints;
    }

    private static Task<IResult> UpdateUserState(
        Guid userId,
        UpdateUserStateRequest request,
        IGatewayProcessor<IdentityModuleConfiguration> gatewayProcessor,
        CancellationToken cancellationToken)
    {
        return gatewayProcessor.ExecuteCommand(async commandProcessor =>
        {
            var command = new UpdateUserState(userId, request.UserState);

            await commandProcessor.SendAsync(command, cancellationToken);

            return Results.NoContent();
        });
    }
}
