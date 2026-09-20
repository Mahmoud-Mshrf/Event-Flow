using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Identity;
using EventFlow.Domain.Users;
using EventFlow.Domain.Users.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Authentication.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler(
    IAppDbContext db,
    IPasswordHasher passwordHasher) : IRequestHandler<ResetPasswordCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        // 1. Find user by email
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, ct);

        if (user is null)
            return UserErrors.NotFound;

        // 2. Find the most recent unused PasswordReset token for this user
        var token = await db.VerificationTokens
            .Where(t => t.UserId == user.Id
                && t.Type == VerificationTokenType.PasswordReset
                && !t.Used)
            .OrderByDescending(t => t.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);

        if (token is null)
            return VerificationTokenErrors.NotFound;

        // 3. Verify the raw code against stored hash — infrastructure concern, lives here
        if (!passwordHasher.Verify(request.Code, token.CodeHash))
            return VerificationTokenErrors.InvalidCode;

        // 4. Consume the token — domain enforces used/expired rules
        var consumeResult = token.Consume(DateTime.UtcNow);
        if (consumeResult.IsError)
            return consumeResult.TopError;

        // 5. Reset the password
        var newHash = passwordHasher.Hash(request.NewPassword);
        var resetResult = user.ResetPassword(newHash);
        if (resetResult.IsError)
            return resetResult.TopError;

        // 6. Revoke ALL active refresh tokens — logout everywhere
        //    Any session that existed before this reset is now invalid
        var activeRefreshTokens = await db.RefreshTokens
            .Where(rt => rt.UserId == user.Id
                && rt.RevokedAtUtc == null)
            .ToListAsync(ct);

        foreach (var refreshToken in activeRefreshTokens)
            refreshToken.Revoke(replacedByTokenId: null);

        // 7. Raise event — notify user via email, non-blocking
        user.AddDomainEvent(new PasswordResetCompletedDomainEvent(user.Email));

        await db.SaveChangesAsync(ct);

        return Result.Success;
    }
}