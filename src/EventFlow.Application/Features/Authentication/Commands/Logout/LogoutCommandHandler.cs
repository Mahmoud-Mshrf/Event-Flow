using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Identity;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Authentication.Commands.Logout;

public sealed class LogoutCommandHandler(
    IAppDbContext db,
    IPasswordHasher passwordHasher,
    ICurrentUser currentUser) : IRequestHandler<LogoutCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(LogoutCommand request, CancellationToken ct)
    {
        // 1. Confirm the caller is authenticated
        if (!currentUser.IsAuthenticated)
            return LogoutErrors.Unauthenticated;

        // 2. Load active refresh tokens for this user only
        //    Scoping to the current user prevents one user revoking another's token
        //    even if they somehow obtained the raw token value
        var activeTokens = await db.RefreshTokens
            .Where(rt => rt.UserId == currentUser.UserId
                && rt.RevokedAtUtc == null)
            .ToListAsync(ct);

        // 3. Find the matching token by verifying against each stored hash
        var matchedToken = activeTokens
            .FirstOrDefault(rt => passwordHasher.Verify(request.RefreshToken, rt.TokenHash));

        if (matchedToken is null)
            return RefreshTokenErrors.NotFoundOrInactive;

        // 4. Revoke it — no replacement token since this is intentional logout
        var revokeResult = matchedToken.Revoke(replacedByTokenId: null);
        if (revokeResult.IsError)
            return revokeResult.TopError;

        await db.SaveChangesAsync(ct);

        return Result.Success;
    }
}
