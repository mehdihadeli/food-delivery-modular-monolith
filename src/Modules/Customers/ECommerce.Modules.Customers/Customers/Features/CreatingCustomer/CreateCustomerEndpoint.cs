using Ardalis.GuardClauses;
using Asp.Versioning.Conventions;
using BuildingBlocks.Abstractions.Web;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Modules.Customers.Customers.Features.CreatingCustomer;

public class CreateCustomerEndpoint : IMinimalEndpointDefinition
{
    public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost(CustomersConfigs.CustomersPrefixUri, CreateCustomer)
            .AllowAnonymous()
            .Produces<CreateCustomerResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithTags(CustomersConfigs.Tag)
            .WithMetadata(new SwaggerOperationAttribute("Creating a Customer", "Creating a Customer"))
            .WithName("CreateCustomer")
            .WithDisplayName("Register New Customer.")
            .WithApiVersionSet(CustomersConfigs.VersionSet)
            .HasApiVersion(1.0)
            .HasApiVersion(2.0);

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
