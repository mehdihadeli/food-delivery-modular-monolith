using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.Web;
using BuildingBlocks.Abstractions.Web.MinimalApi;

namespace ECommerce.Modules.Customers.Customers.Features.CreatingCustomer;

public class CreateCustomerEndpoint : IMinimalEndpointDefinition
{
    public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost(CustomersConfigs.CustomersPrefixUri, CreateCustomer)
            .AllowAnonymous()
            .WithTags(CustomersConfigs.Tag)
            .Produces<CreateCustomerResult>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("CreateCustomer")
            .WithDisplayName("Register New Customer.");

        return builder;
    }

    private static Task<IResult> CreateCustomer(
        CreateCustomerRequest request,
        IGatewayProcessor<CustomersModuleConfiguration> gatewayProcessor,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(request, nameof(request));

        return gatewayProcessor.ExecuteCommand(async commandProcessor =>
        {
            var command = new CreateCustomer(request.Email);

            var result = await commandProcessor.SendAsync(command, cancellationToken);

            return Results.Created($"{CustomersConfigs.CustomersPrefixUri}/{result.CustomerId}", result);
        });
    }
}
