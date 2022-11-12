using System.Security.Claims;
using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Command;
using BuildingBlocks.Security.Jwt;
using ECommerce.Modules.Identity.Identity.Exceptions;
using ECommerce.Modules.Identity.Identity.Features.GenerateJwtToken;
using ECommerce.Modules.Identity.Identity.Features.GenerateRefreshToken;
using ECommerce.Modules.Identity.Shared.Exceptions;
using ECommerce.Modules.Identity.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace ECommerce.Modules.Identity.Identity.Features.RefreshingToken;

public record RefreshToken(string AccessTokenData, string RefreshTokenData) : ICommand<RefreshTokenResponse>;

internal class RefreshTokenValidator : AbstractValidator<RefreshToken>
{
    public RefreshTokenValidator()
    {
        RuleFor(v => v.AccessTokenData)
            .NotEmpty();

        RuleFor(v => v.RefreshTokenData)
            .NotEmpty();
    }
}

internal class RefreshTokenHandler : ICommandHandler<RefreshToken, RefreshTokenResponse>
{
    private readonly ICommandProcessor _commandProcessor;
    private readonly IJwtService _jwtService;
    private readonly UserManager<ApplicationUser> _userManager;

    public RefreshTokenHandler(
        IJwtService jwtService,
        UserManager<ApplicationUser> userManager,
        ICommandProcessor commandProcessor)
    {
        _jwtService = jwtService;
        _userManager = userManager;
        _commandProcessor = commandProcessor;
    }

    public async Task<RefreshTokenResponse> Handle(RefreshToken request, CancellationToken cancellationToken)
    {
        Guard.Against.Null(request, nameof(RefreshToken));

        // invalid token/signing key was passed and we can't extract user claims
        var userClaimsPrincipal = _jwtService.GetPrincipalFromToken(request.AccessTokenData);

        if (userClaimsPrincipal is null)
            throw new InvalidTokenException(userClaimsPrincipal);

        var userId = userClaimsPrincipal.FindFirstValue(JwtRegisteredClaimNames.NameId);

        var identityUser = await _userManager.FindByIdAsync(userId);

        if (identityUser == null)
            throw new UserNotFoundException(userId);

        var refreshToken =
            (await _commandProcessor.SendAsync(
                new GenerateRefreshToken.GenerateRefreshToken { UserId = identityUser.Id, Token = request.RefreshTokenData },
                cancellationToken)).RefreshToken;

        var accessToken =
            await _commandProcessor.SendAsync(
                new GenerateJwtToken.GenerateJwtToken(identityUser, refreshToken.Token), cancellationToken);

        return new RefreshTokenResponse(identityUser, accessToken, refreshToken.Token);
    }
}
