using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Command;
using ECommerce.Modules.Identity.Identity.Exceptions;
using ECommerce.Modules.Identity.Identity.Features.RefreshingToken;
using ECommerce.Modules.Identity.Shared.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Identity.Identity.Features.RevokeRefreshToken;

public record RevokeRefreshTokenCommand(string RefreshToken) : ICommand;

internal class RevokeRefreshTokenCommandHandler : ICommandHandler<RevokeRefreshTokenCommand>
{
    private readonly IdentityContext _context;

    public RevokeRefreshTokenCommandHandler(IdentityContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(
        RevokeRefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(request, nameof(RevokeRefreshTokenCommand));

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
