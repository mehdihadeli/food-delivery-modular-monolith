using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.Web;
using BuildingBlocks.Abstractions.Web.MinimalApi;

namespace ECommerce.Modules.Customers.RestockSubscriptions.Features.GettingRestockSubscriptionById;

public class GetRestockSubscriptionByIdEndpoint : IMinimalEndpointDefinition
{
    public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet(
                $"{RestockSubscriptionsConfigs.RestockSubscriptionsUrl}/{{id}}",
                GetRestockSubscriptionById)
            .WithTags(RestockSubscriptionsConfigs.Tag)
            .RequireAuthorization(CustomersConstants.Role.Admin)
            .Produces<GetRestockSubscriptionByIdResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("GetRestockSubscriptionById")
            .WithDisplayName("Get RestockSubscription By Id.");

        return builder;
    }

    private static Task<IResult> GetRestockSubscriptionById(
        long id,
        IGatewayProcessor<CustomersModuleConfiguration> gatewayProcessor,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(id, nameof(id));

        return gatewayProcessor.ExecuteQuery(async queryProcessor =>
        {
            var result = await queryProcessor.SendAsync(new GetRestockSubscriptionById(id), cancellationToken);

            return Results.Ok(result);
        });
    }
}
