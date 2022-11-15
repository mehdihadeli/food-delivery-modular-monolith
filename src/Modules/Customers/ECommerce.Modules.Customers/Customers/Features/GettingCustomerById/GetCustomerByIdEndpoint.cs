using Ardalis.GuardClauses;
using Asp.Versioning.Conventions;
using BuildingBlocks.Abstractions.Web;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using Swashbuckle.AspNetCore.Annotations;

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
            .WithTags(CustomersConfigs.Tag)
            .WithMetadata(new SwaggerOperationAttribute("Getting a Customer By Id", "Getting a Customer By Id"))
            .WithName("GetCustomerById")
            .WithDisplayName("Get Customer By Id.")
            .WithApiVersionSet(CustomersConfigs.VersionSet)
            .HasApiVersion(1.0);

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
