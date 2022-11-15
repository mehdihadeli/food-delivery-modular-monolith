using Ardalis.GuardClauses;
using Asp.Versioning.Conventions;
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
            .RequireAuthorization(CustomersConstants.Role.Admin)
            .Produces<GetRestockSubscriptionByIdResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags(RestockSubscriptionsConfigs.Tag)
            .WithName("GetRestockSubscriptionById")
            .WithDisplayName("Get RestockSubscription By Id.")
            .WithApiVersionSet(RestockSubscriptionsConfigs.VersionSet)
            .HasApiVersion(1.0);

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
