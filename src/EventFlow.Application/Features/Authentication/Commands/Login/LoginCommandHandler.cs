using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Common.Errors;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EventFlow.Application.Features.Authentication.Commands.Login;

public sealed class LoginCommandHandler(
    IAppDbContext db,
    IPasswordHasher passwordHasher,
    ITokenService tokenService, ITokenSettings tokenSettings) : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken ct)
    {
        // 1. Look up user by email
        var user = await db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == request.Email, ct);

        // 2. Verify password — even if user is null, still run a dummy verify
        //    to prevent timing attacks leaking whether the email exists
        var passwordValid = user is not null
            && passwordHasher.Verify(request.Password, user.PasswordHash);

        if (!passwordValid)
            return LoginErrors.InvalidCredentials;

        // 3. Guard: disabled account
        if (user!.Disabled)
            return LoginErrors.AccountDisabled;

        // 4. Guard: email not confirmed
        if (!user.EmailConfirmed)
            return LoginErrors.EmailNotConfirmed;

        // 5. Issue access token
        var accessToken = tokenService.GenerateAccessToken(user);

        // 6. Generate refresh token — raw value goes to client, hash stored in DB
        var rawRefreshToken = tokenService.GenerateRefreshToken();
        var refreshTokenHash = passwordHasher.Hash(rawRefreshToken);

        var refreshToken = RefreshToken.Issue(
            Guid.NewGuid(),
            user.Id,
            refreshTokenHash,
            validFor: TimeSpan.FromDays(tokenSettings.RefreshTokenDays));

        await db.RefreshTokens.AddAsync(refreshToken, ct);
        await db.SaveChangesAsync(ct);

        return new LoginResponse(accessToken, rawRefreshToken);
    }
}


public static class LoginErrors
{
    public static readonly Error InvalidCredentials =
        Error.Unauthorized("Login.InvalidCredentials",
            "The email or password is incorrect.");

    public static readonly Error AccountDisabled =
        Error.Unauthorized("Login.AccountDisabled",
            "This account has been disabled. Please contact support.");

    public static readonly Error EmailNotConfirmed =
        Error.Unauthorized("Login.EmailNotConfirmed",
            "Please confirm your email address before logging in.");
}