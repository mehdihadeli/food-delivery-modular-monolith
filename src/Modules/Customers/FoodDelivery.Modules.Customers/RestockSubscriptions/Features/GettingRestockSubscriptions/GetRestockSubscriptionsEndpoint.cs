using Ardalis.ApiEndpoints;
using Ardalis.GuardClauses;
using Asp.Versioning;
using Squirrel.Abstractions.Queries;
using Swashbuckle.AspNetCore.Annotations;

namespace FoodDelivery.Modules.Customers.RestockSubscriptions.Features.GettingRestockSubscriptions;

// https://www.youtube.com/watch?v=SDu0MA6TmuM
// https://github.com/ardalis/ApiEndpoints
public class GetRestockSubscriptionsEndpoint : EndpointBaseAsync
    .WithRequest<GetRestockSubscriptionsRequest?>
    .WithActionResult<GetRestockSubscriptionsResponse>
{
    private readonly IQueryBus _queryBus;

    public GetRestockSubscriptionsEndpoint(IQueryBus queryBus)
    {
        _queryBus = queryBus;
    }

    [HttpGet(RestockSubscriptionsConfigs.RestockSubscriptionsUrl, Name = "GetRestockSubscriptions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ApiVersion(1.0)]
    [SwaggerOperation(
        Summary = "Get Restock Subscriptions.",
        Description = "Get Restock Subscriptions.",
        OperationId = "GetRestockSubscriptions",
        Tags = new[] { RestockSubscriptionsConfigs.Tag })]
    [Authorize(Roles = CustomersConstants.Role.Admin)]
    public override async Task<ActionResult<GetRestockSubscriptionsResponse>> HandleAsync(
        [FromQuery] GetRestockSubscriptionsRequest? request,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(request, nameof(request));

        var result = await _queryBus.SendAsync(
            new GetRestockSubscriptions
            {
                PageNumber = request.PageNumber,
                SortOrder = request.SortOrder,
                PageSize = request.PageSize,
                Filters = request.Filters,
                Emails = request.Emails,
                From = request.From,
                To = request.To
            },
            cancellationToken);

        return Ok(result);
    }
}
