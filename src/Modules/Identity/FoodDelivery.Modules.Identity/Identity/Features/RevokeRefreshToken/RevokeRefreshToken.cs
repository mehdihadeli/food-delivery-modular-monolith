using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Command;
using FoodDelivery.Modules.Identity.Identity.Exceptions;
using FoodDelivery.Modules.Identity.Identity.Features.RefreshingToken;
using FoodDelivery.Modules.Identity.Shared.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Modules.Identity.Identity.Features.RevokeRefreshToken;

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

        var refreshToken = await _context.Set<global::FoodDelivery.Modules.Identity.Shared.Models.RefreshToken>()
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
