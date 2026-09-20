using System.Security.Cryptography;
using EventFlow.Application.Common.Helpers;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Identity;
using EventFlow.Domain.Users.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Authentication.Commands.RequestPasswordReset;

public sealed class RequestPasswordResetCommandHandler(
    IAppDbContext db,
    IPasswordHasher passwordHasher) : IRequestHandler<RequestPasswordResetCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(RequestPasswordResetCommand request, CancellationToken ct)
    {
        // 1. Look up the user — but never reveal whether the email exists or not
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, ct);

        // 2. Always return success even if user not found
        //    This prevents email enumeration — an attacker gets no signal
        //    either way. The OTP is only sent if the account actually exists.
        if (user is null)
            return Result.Success;

        // 3. Rate-limit: reject if a fresh token already exists
        //    "Fresh" = created in the last 2 minutes, regardless of Used status
        //    This prevents OTP-spamming without revealing account existence
        var recentTokenExists = await db.VerificationTokens
            .AnyAsync(t => t.UserId == user.Id
                && t.Type == VerificationTokenType.PasswordReset
                && t.CreatedAtUtc >= DateTimeOffset.UtcNow.AddMinutes(-2), ct);

        if (recentTokenExists)
            return Result.Success; // silent — same response as success

        // 4. Generate OTP and hash it
        var rawCode = OtpGenerator.Generate();
        var codeHash = passwordHasher.Hash(rawCode);

        var token = VerificationToken.Create(
            Guid.NewGuid(),
            user.Id,
            VerificationTokenType.PasswordReset,
            codeHash,
            validFor: TimeSpan.FromMinutes(15));

        // 5. Raise domain event — email sends after commit, non-blocking
        user.AddDomainEvent(new PasswordResetRequestedDomainEvent(user.Email, rawCode));

        await db.VerificationTokens.AddAsync(token, ct);
        await db.SaveChangesAsync(ct);

        return Result.Success;
    }
}

