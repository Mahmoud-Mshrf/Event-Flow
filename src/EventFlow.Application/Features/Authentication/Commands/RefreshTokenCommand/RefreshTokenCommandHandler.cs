using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Authentication.Commands.Login;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Identity;
using MediatR;

namespace EventFlow.Application.Features.Authentication.Commands.RefreshTokenCommand;

public class RefreshTokenCommandHandler(
    ITokenService tokenService,
    IPasswordHasher passwordHasher,
    IAppDbContext db,ITokenSettings tokenSettings) : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        // 1. Look up refresh token by hash
        var refreshTokenHash = passwordHasher.Hash(request.RefreshToken);
        var refreshToken = await db.RefreshTokens
            .FindAsync(new object?[] { refreshTokenHash }, ct);

        // 2. Guard: not found or expired
        if (refreshToken is null || !refreshToken.IsActive)
            return RefreshTokenErrors.InvalidRefreshToken;

        // 3. Look up user by ID
        var user = await db.Users.FindAsync(refreshToken.UserId);

        // 4. Guard: user not found or disabled
        if (user is null || user.Disabled)
            return RefreshTokenErrors.NotFoundOrInactive;

        // 5. Issue access token
        var accessToken = tokenService.GenerateAccessToken(user);

        // 6. Generate refresh token — raw value goes to client, hash stored in DB
        var rawRefreshToken = tokenService.GenerateRefreshToken();
        var newRefreshTokenHash = passwordHasher.Hash(rawRefreshToken);

        var newRefreshToken = RefreshToken.Issue(
            Guid.NewGuid(),
            user.Id,
            newRefreshTokenHash,
            validFor: TimeSpan.FromDays(tokenSettings.RefreshTokenDays));
        refreshToken.Revoke(replacedByTokenId: newRefreshToken.Id);

        return new LoginResponse(accessToken,rawRefreshToken);
    }
}
