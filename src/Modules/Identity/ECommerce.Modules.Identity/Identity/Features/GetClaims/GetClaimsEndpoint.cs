using Asp.Versioning.Conventions;
using BuildingBlocks.Abstractions.Web;

namespace ECommerce.Modules.Identity.Identity.Features.GetClaims;

public static class GetClaimsEndpoint
{
    internal static IEndpointRouteBuilder MapGetClaimsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet($"{IdentityConfigs.IdentityPrefixUri}/claims", GetClaims)
            .WithTags(IdentityConfigs.Tag)
            .RequireAuthorization()
            .Produces<GetClaimsResponse>()
            .Produces(StatusCodes.Status401Unauthorized)
            .WithDisplayName("Get User claims")
            .WithApiVersionSet(IdentityConfigs.VersionSet)
            .HasApiVersion(1.0);

        return endpoints;
    }

    private static Task<IResult> GetClaims(
        IGatewayProcessor<IdentityModuleConfiguration> gatewayProcessor,
        CancellationToken cancellationToken)
    {
       return gatewayProcessor.ExecuteQuery(async queryProcessor =>
       {
           var result = await queryProcessor.SendAsync(new GetClaims(), cancellationToken);

           return Results.Ok(result);
       });
    }
}
