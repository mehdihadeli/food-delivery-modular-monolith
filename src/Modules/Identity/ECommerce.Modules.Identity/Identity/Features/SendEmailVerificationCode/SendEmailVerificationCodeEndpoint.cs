using BuildingBlocks.Abstractions.Web;

namespace ECommerce.Modules.Identity.Identity.Features.SendEmailVerificationCode;

public static class SendEmailVerificationCodeEndpoint
{
    internal static IEndpointRouteBuilder MapSendEmailVerificationCodeEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
                $"{IdentityConfigs.IdentityPrefixUri}/send-email-verification-code",
                SendEmailVerificationCode)
            .WithTags(IdentityConfigs.Tag)
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status400BadRequest)
            .WithDisplayName("Send Email Verification Code.");

        return endpoints;
    }

    private static Task<IResult> SendEmailVerificationCode(
        SendEmailVerificationCodeRequest request,
        IGatewayProcessor<IdentityModuleConfiguration> gatewayProcessor,
        CancellationToken cancellationToken)
    {
       return gatewayProcessor.ExecuteCommand(async commandProcessor =>
       {
           var command = new SendEmailVerificationCodeCommand(request.Email);

           var result = await commandProcessor.SendAsync(command, cancellationToken);

           return Results.Ok(result);
       });
    }
}
