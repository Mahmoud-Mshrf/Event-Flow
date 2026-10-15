using System.Security.Cryptography;
using EventFlow.Application.Common.Helpers;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Settings;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Identity;
using EventFlow.Domain.Users.Events;
using MassTransit.Configuration;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EventFlow.Application.Features.Authentication.Commands.ResendEmailConfirmation;

public sealed class ResendEmailConfirmationCommandHandler(
    IAppDbContext db,
    IPasswordHasher passwordHasher,IOptions<VerificationTokenSettings> tokenSettings) : IRequestHandler<ResendEmailConfirmationCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(ResendEmailConfirmationCommand request, CancellationToken ct)
    {
        var settings = tokenSettings.Value;
        var expiry = TimeSpan.FromMinutes(settings.PasswordResetExpiryMinutes);
        // 1. Look up user — same silent-success pattern as RequestPasswordReset
        //    Never reveal whether this email is registered
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, ct);

        if (user is null)
            return Result.Success;

        // 2. Already confirmed — nothing to resend, but still silent
        //    Returning a specific error here would let an attacker confirm
        //    which emails are registered AND confirmed
        if (user.EmailConfirmed)
            return Result.Success;

        // 3. Rate-limit: block if a token was issued in the last 2 minutes
        var recentTokenExists = await db.VerificationTokens
            .AnyAsync(t => t.UserId == user.Id
                && t.Type == VerificationTokenType.EmailConfirmation
                && t.CreatedAtUtc >= DateTimeOffset.UtcNow.AddMinutes(-2), ct);

        if (recentTokenExists)
            return Result.Success;

        // 4. Generate fresh OTP and hash it
        var rawCode = OtpGenerator.Generate();
        var codeHash = passwordHasher.Hash(rawCode);

        var token = VerificationToken.Create(
            Guid.NewGuid(),
            user.Id,
            VerificationTokenType.EmailConfirmation,
            codeHash,
            validFor: expiry);

        // 5. Raise domain event — email sends after commit, non-blocking
        user.AddDomainEvent(new EmailConfirmationResentDomainEvent(user.Email, rawCode));

        await db.VerificationTokens.AddAsync(token, ct);
        await db.SaveChangesAsync(ct);

        return Result.Success;
    }
}