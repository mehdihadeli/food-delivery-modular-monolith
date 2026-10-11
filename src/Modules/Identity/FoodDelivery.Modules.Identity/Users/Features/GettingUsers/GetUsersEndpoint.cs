using Ardalis.ApiEndpoints;
using Ardalis.GuardClauses;
using Asp.Versioning;
using Squirrel.Abstractions.Queries;
using Squirrel.Abstractions.Web;
using Squirrel.Validation.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace FoodDelivery.Modules.Identity.Users.Features.GettingUsers;

// https://www.youtube.com/watch?v=SDu0MA6TmuM
// https://github.com/ardalis/ApiEndpoints
public class GetUsersEndpoint : EndpointBaseAsync
    .WithRequest<GetUsersRequest?>
    .WithActionResult<GetUsersResponse>
{
    private readonly IGatewayProcessor<IdentityModuleConfiguration> _gatewayProcessor;

    public GetUsersEndpoint(IGatewayProcessor<IdentityModuleConfiguration> gatewayProcessor)
    {
        _gatewayProcessor = gatewayProcessor;
    }

    [HttpGet(UsersConfigs.UsersPrefixUri, Name = "GetUsers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ApiVersion(1.0)]
    [SwaggerOperation(
        Summary = "Get all users",
        Description = "Get all users",
        OperationId = "GetUsers",
        Tags = new[] { UsersConfigs.Tag })]
    public override async Task<ActionResult<GetUsersResponse>> HandleAsync(
        [FromQuery] GetUsersRequest? request,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(request, nameof(request));

        var query = new GetUsers
            {
                Filters = request.Filters,
                PageNumber = request.PageNumber,
                SortOrder = request.SortOrder,
                PageSize = request.PageSize
            };
        new GetUsersValidator().HandleValidation(query);
        var result = await _gatewayProcessor.ExecuteQuery(async queryBus => await queryBus.SendAsync(query, cancellationToken));

        return Ok(result);
    }
}
