using Ardalis.GuardClauses;
using Asp.Versioning.Conventions;
using BuildingBlocks.Abstractions.Web;
using ECommerce.Modules.Catalogs.Products.Features.CreatingProduct.Requests;

namespace ECommerce.Modules.Catalogs.Products.Features.CreatingProduct;

// POST api/v1/catalog/products
public static class CreateProductEndpoint
{
    internal static IEndpointRouteBuilder MapCreateProductsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        // https://github.com/dotnet/aspnetcore/issues/45082
        // https://github.com/dotnet/aspnetcore/issues/40753
        // https://github.com/domaindrivendev/Swashbuckle.AspNetCore/pull/2414
        endpoints.MapPost($"{ProductsConfigs.ProductsPrefixUri}", CreateProducts)
            // WithOpenApi should placed before versioning and other things
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Creating a New Product", Description = "Creating a New Product"
            })
            .WithTags(ProductsConfigs.Tag)
            .RequireAuthorization()
            .Produces<CreateProductResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("CreateProduct")
            .WithDisplayName("Create a new product.")

            // .WithMetadata(new SwaggerOperationAttribute("Creating a New Product", "Creating a New Product"))
            .WithApiVersionSet(ProductsConfigs.VersionSet)

            // .IsApiVersionNeutral()
            // .MapToApiVersion(1.0)
            .HasApiVersion(1.0)
            .HasApiVersion(2.0);

        return endpoints;
    }

    private static Task<IResult> CreateProducts(
        CreateProductRequest request,
        IGatewayProcessor<CatalogModuleConfiguration> gatewayProcessor,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(request, nameof(request));

        return gatewayProcessor.ExecuteCommand(async (commandProcessor, mapper) =>
        {
            var command = mapper.Map<CreateProduct>(request);
            var result = await commandProcessor.SendAsync(command, cancellationToken);

            return Results.CreatedAtRoute("GetProductById", new {id = result.Product.Id}, result);
        });
    }
}
