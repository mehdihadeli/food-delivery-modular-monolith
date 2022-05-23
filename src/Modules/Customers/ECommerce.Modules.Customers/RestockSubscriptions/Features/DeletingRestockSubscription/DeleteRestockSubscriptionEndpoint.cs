using BuildingBlocks.Abstractions.Web;
using BuildingBlocks.Abstractions.Web.MinimalApi;

namespace ECommerce.Modules.Customers.RestockSubscriptions.Features.DeletingRestockSubscription;

public class DeleteRestockSubscriptionEndpoint : IMinimalEndpointDefinition
{
    public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapDelete($"{RestockSubscriptionsConfigs.RestockSubscriptionsUrl}/{{id}}", DeleteRestockSubscription)
            .RequireAuthorization(CustomersConstants.Role.Admin)
            .WithTags(RestockSubscriptionsConfigs.Tag)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithName("DeleteRestockSubscription")
            .WithDisplayName("Delete RestockSubscription for Customer.");

        return builder;
    }

    private static Task<IResult> DeleteRestockSubscription(
        long id,
        IGatewayProcessor<CustomersModuleConfiguration> gatewayProcessor,
        CancellationToken cancellationToken)
    {
        return gatewayProcessor.ExecuteCommand(async commandProcessor =>
        {
            var command = new DeleteRestockSubscription(id);

            await commandProcessor.SendAsync(command, cancellationToken);

            return Results.NoContent();
        });
    }
}
