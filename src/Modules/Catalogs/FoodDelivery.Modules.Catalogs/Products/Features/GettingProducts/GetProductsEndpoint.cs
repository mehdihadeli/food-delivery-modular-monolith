using Ardalis.ApiEndpoints;
using Ardalis.GuardClauses;
using Squirrel.Abstractions.Queries;
using Squirrel.Abstractions.Web;
using Swashbuckle.AspNetCore.Annotations;

namespace FoodDelivery.Modules.Catalogs.Products.Features.GettingProducts;

// https://www.youtube.com/watch?v=SDu0MA6TmuM
// https://github.com/ardalis/ApiEndpoints
public class GetProductsEndpoint : EndpointBaseAsync
    .WithRequest<GetProductsRequest?>
    .WithActionResult<GetProductsResponse>
{
    private readonly IGatewayProcessor<CatalogModuleConfiguration> _gatewayProcessor;

    public GetProductsEndpoint(IGatewayProcessor<CatalogModuleConfiguration> gatewayProcessor)
    {
        _gatewayProcessor = gatewayProcessor;
    }

    [HttpGet(ProductsConfigs.ProductsPrefixUri, Name = "GetProducts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerOperation(
        Summary = "Get all products",
        Description = "Get all products",
        OperationId = "GetProducts",
        Tags = new[] { ProductsConfigs.Tag })]
    public override async Task<ActionResult<GetProductsResponse>> HandleAsync(
        [FromQuery] GetProductsRequest? request,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(request, nameof(request));


        var query = GetProducts.Of(request);
        var result = await _gatewayProcessor.ExecuteQuery(async queryBus => await queryBus.SendAsync(query, cancellationToken));

        return Ok(result);
    }
}
