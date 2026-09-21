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
    ICurrentUser currentUser,ITokenService tokenService,ITokenSettings tokenSettings) : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        // 1. Confirm the caller is authenticated
        if (!currentUser.IsAuthenticated)
            return RefreshTokenErrors.Unauthenticated;

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

        // 4. Issue access token
        var user = await db.Users.FindAsync(matchedToken.UserId);
        if (user is null || user.Disabled)
            return RefreshTokenErrors.NotFoundOrInactive;
        var accessToken = tokenService.GenerateAccessToken(user);
        var rawRefreshToken = tokenService.GenerateRefreshToken();
        var newRefreshTokenHash = passwordHasher.Hash(rawRefreshToken);
        var newRefreshToken = RefreshToken.Issue(
            Guid.NewGuid(),
            user.Id,
            newRefreshTokenHash,
            validFor: TimeSpan.FromDays(tokenSettings.RefreshTokenDays));
        // 5. Revoke it — replace with a new token since this is a refresh operation
        var revokeResult = matchedToken.Revoke(replacedByTokenId: newRefreshToken.Id);
        if (revokeResult.IsError)
            return revokeResult.TopError;
        await db.RefreshTokens.AddAsync(newRefreshToken,ct);
        await db.SaveChangesAsync(ct);
        return new LoginResponse(accessToken, rawRefreshToken);
    }
}