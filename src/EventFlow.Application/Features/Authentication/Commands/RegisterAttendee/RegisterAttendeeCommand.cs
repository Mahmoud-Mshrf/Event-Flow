using System.Security.Cryptography;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Identity;
using EventFlow.Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Authentication.Commands.RegisterAttendee;  
public sealed record RegisterAttendeeCommand(
    string Name,
    string Email,
    string PhoneNumber,
    string Password) : IRequest<Result<Success>>;

public sealed class RegisterAttendeeCommandHandler(
    IAppDbContext db,
    IPasswordHasher passwordHasher,
    IEmailSender emailSender) : IRequestHandler<RegisterAttendeeCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(RegisterAttendeeCommand request, CancellationToken ct)
    {
        // 1. Check email uniqueness across the whole system
        var emailExists = await db.Users
            .AnyAsync(u => u.Email == request.Email, ct);

        if (emailExists)
            return UserErrors.EmailAlreadyInUse;

        // 2. Hash password before touching the domain
        var passwordHash = passwordHasher.Hash(request.Password);

        // 3. Create the User aggregate
        var userResult = User.CreateAttendee(
            Guid.NewGuid(),
            request.PhoneNumber,
            request.Name,
            request.Email,
            passwordHash);

        if (userResult.IsError)
            return userResult.TopError;

        var user = userResult.Value;

        // 4. Generate a 6-digit OTP and hash it before storing
        var rawCode = GenerateOtp();
        var codeHash = passwordHasher.Hash(rawCode);

        var verificationToken = VerificationToken.Create(
            Guid.NewGuid(),
            user.Id,
            VerificationTokenType.EmailConfirmation,
            codeHash,
            validFor: TimeSpan.FromHours(24));

        // 5. Persist both in one transaction
        await db.Users.AddAsync(user, ct);
        await db.VerificationTokens.AddAsync(verificationToken, ct);
        await db.SaveChangesAsync(ct);

        // 6. Send confirmation email — after commit, non-blocking
        //    If this fails the user still exists and can request a resend
        await emailSender.SendEmailConfirmationAsync(user.Email, rawCode, ct);

        return Result.Success;
    }

    private static string GenerateOtp()
    {
        // Cryptographically random 6-digit code
        var randomNumber = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return randomNumber.ToString("D6");
    }
}