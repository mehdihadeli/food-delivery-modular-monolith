using Ardalis.GuardClauses;
using Asp.Versioning.Conventions;
using BuildingBlocks.Abstractions.Web;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Modules.Catalogs.Products.Features.GettingProductById;

// GET api/v1/catalog/products/{id}
public static class GetProductByIdEndpoint
{
    internal static IEndpointRouteBuilder MapGetProductByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
                $"{ProductsConfigs.ProductsPrefixUri}/{{id}}",
                GetProductById)
            // .RequireAuthorization()
            .Produces<GetProductByIdResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags(ProductsConfigs.Tag)
            .WithMetadata(new SwaggerOperationAttribute("Getting Product by Id", "Getting Product by Id"))
            .WithName("GetProductById")
            .WithDisplayName("Get product By Id.")
            .WithApiVersionSet(ProductsConfigs.VersionSet)
            .MapToApiVersion(1.0)
            .HasApiVersion(1.0);

        return endpoints;
    }

    private static Task<IResult> GetProductById(
        long id,
        IGatewayProcessor<CatalogModuleConfiguration> gatewayProcessor,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(id, nameof(id));

        return gatewayProcessor.ExecuteQuery(async queryProcessor =>
        {
            var result = await queryProcessor.SendAsync(new GetProductById(id), cancellationToken);

            return Results.Ok(result);
        });
    }
}
