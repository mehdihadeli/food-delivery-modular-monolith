using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.Web;
using BuildingBlocks.Abstractions.Web.MinimalApi;

namespace ECommerce.Modules.Customers.Customers.Features.GettingCustomerById;

public class GetCustomerByIdEndpointEndpoint : IMinimalEndpointDefinition
{
    public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet(
                $"{CustomersConfigs.CustomersPrefixUri}/{{id}}",
                GetCustomerById)
            .WithTags(CustomersConfigs.Tag)
            // .RequireAuthorization()
            .Produces<GetCustomerByIdResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("GetCustomerById")
            .WithDisplayName("Get Customer By Id.");

        return builder;
    }

    private static Task<IResult> GetCustomerById(
        long id,
        IGatewayProcessor<CustomersModuleConfiguration> gatewayProcessor,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(id, nameof(id));

        return gatewayProcessor.ExecuteQuery(async queryProcessor =>
        {
            var result = await queryProcessor.SendAsync(new GetCustomerById(id), cancellationToken);

            return Results.Ok(result);
        });
    }
}
