using BuildingBlocks.Abstractions.CQRS.Query;
using Microsoft.AspNetCore.Http;

namespace ECommerce.Modules.Identity.Identity.Features.GetClaims;

public class GetClaims : IQuery<GetClaimsResponse>
{
}

public class GetClaimsHandler : IQueryHandler<GetClaims, GetClaimsResponse>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetClaimsHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<GetClaimsResponse> Handle(GetClaims request, CancellationToken cancellationToken)
    {
        var claims = _httpContextAccessor.HttpContext?.User.Claims.Select(x => new ClaimDto
        {
            Type = x.Type, Value = x.Value
        });

        return Task.FromResult(new GetClaimsResponse(claims));
    }
}
