using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Command;
using ECommerce.Modules.Identity.Identity.Exceptions;
using ECommerce.Modules.Identity.Identity.Features.RefreshingToken;
using ECommerce.Modules.Identity.Shared.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Identity.Identity.Features.RevokeRefreshToken;

public record RevokeRefreshToken(string RefreshToken) : ICommand;

internal class RevokeRefreshTokenHandler : ICommandHandler<RevokeRefreshToken>
{
    private readonly IdentityContext _context;

    public RevokeRefreshTokenHandler(IdentityContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(
        RevokeRefreshToken request,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(request, nameof(RevokeRefreshToken));

        var refreshToken = await _context.Set<global::ECommerce.Modules.Identity.Shared.Models.RefreshToken>()
            .FirstOrDefaultAsync(x => x.Token == request.RefreshToken, cancellationToken: cancellationToken);

        if (refreshToken == null)
            throw new RefreshTokenNotFoundException(refreshToken);

        if (!refreshToken.IsRefreshTokenValid())
            throw new InvalidRefreshTokenException(refreshToken);

        // revoke token and save
        refreshToken.RevokedAt = DateTime.Now;
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
