using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Authentication.Commands.Login;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Authentication.Commands.RefreshTokenCommand;
public sealed class RefreshTokenCommandHandler(
    IAppDbContext db,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    ITokenSettings tokenSettings) : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(
        RefreshTokenCommand request, CancellationToken ct)
    {
        // 1. Extract UserId from the expired access token
        //    Signature is still validated — only lifetime check is skipped
        var userId = tokenService.GetUserIdFromExpiredToken(request.AccessToken);
        if (userId is null)
            return RefreshTokenErrors.InvalidAccessToken;

        // 2. Load only non-revoked tokens for THIS user — never a cross-user scan
        var activeTokens = await db.RefreshTokens
            .Where(rt => rt.UserId == userId.Value
                && rt.RevokedAtUtc == null)
            .ToListAsync(ct);

        // 3. Find the matching token by verifying hash
        var matchedToken = activeTokens
            .FirstOrDefault(rt => passwordHasher.Verify(request.RefreshToken, rt.TokenHash));

        // 4. Check exists AND still within expiry window
        if (matchedToken is null || !matchedToken.IsActive)
            return RefreshTokenErrors.NotFoundOrInactive;

        // 5. Load user — needed for new access token claims and disabled check
        var user = await db.Users.FindAsync([userId.Value], ct);

        if (user is null || user.Disabled)
            return RefreshTokenErrors.NotFoundOrInactive;

        // 6. Issue new access token and rotate the refresh token
        var accessToken = tokenService.GenerateAccessToken(user);
        var rawRefreshToken = tokenService.GenerateRefreshToken();
        var newRefreshTokenHash = passwordHasher.Hash(rawRefreshToken);

        var newRefreshToken = RefreshToken.Issue(
            Guid.NewGuid(),
            user.Id,
            newRefreshTokenHash,
            validFor: TimeSpan.FromDays(tokenSettings.RefreshTokenDays));

        // 7. Revoke old token, link to replacement for the audit chain
        var revokeResult = matchedToken.Revoke(replacedByTokenId: newRefreshToken.Id);
        if (revokeResult.IsError)
            return revokeResult.TopError;

        await db.RefreshTokens.AddAsync(newRefreshToken, ct);
        await db.SaveChangesAsync(ct);

        return new LoginResponse(accessToken, rawRefreshToken);
    }
}