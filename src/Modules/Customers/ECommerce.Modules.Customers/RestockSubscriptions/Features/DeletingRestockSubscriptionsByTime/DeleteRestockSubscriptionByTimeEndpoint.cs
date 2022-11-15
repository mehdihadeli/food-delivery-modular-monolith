using Ardalis.GuardClauses;
using Asp.Versioning.Conventions;
using BuildingBlocks.Abstractions.Web;
using BuildingBlocks.Abstractions.Web.MinimalApi;

namespace ECommerce.Modules.Customers.RestockSubscriptions.Features.DeletingRestockSubscriptionsByTime;

public class DeleteRestockSubscriptionByTimeEndpoint : IMinimalEndpointDefinition
{
    public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapDelete($"{RestockSubscriptionsConfigs.RestockSubscriptionsUrl}", DeleteRestockSubscriptionByTime)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .WithTags(RestockSubscriptionsConfigs.Tag)
            .WithName("DeleteRestockSubscriptionByTime")
            .WithDisplayName("Delete RestockSubscriptions by time range.")
            .WithApiVersionSet(RestockSubscriptionsConfigs.VersionSet)
            .HasApiVersion(1.0);

        return builder;
    }

    [Authorize(Roles = CustomersConstants.Role.Admin)]
    private static Task<IResult> DeleteRestockSubscriptionByTime(
        [FromBody] DeleteRestockSubscriptionByTimeRequest request,
        IGatewayProcessor<CustomersModuleConfiguration> gatewayProcessor,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(request, nameof(request));

        return gatewayProcessor.ExecuteCommand(async commandProcessor =>
        {
            var command = new DeleteRestockSubscriptionsByTime(request.From, request.To);

            await commandProcessor.SendAsync(command, cancellationToken);

            return Results.NoContent();
        });
    }
}
