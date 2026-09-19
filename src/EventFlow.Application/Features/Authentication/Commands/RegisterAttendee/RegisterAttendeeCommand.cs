using System.Security.Cryptography;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Identity;
using EventFlow.Domain.Users;
using EventFlow.Domain.Users.Events;
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
    IPasswordHasher passwordHasher) : IRequestHandler<RegisterAttendeeCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(RegisterAttendeeCommand request, CancellationToken ct)
    {
        var emailExists = await db.Users
            .AnyAsync(u => u.Email == request.Email, ct);

        if (emailExists)
            return UserErrors.EmailAlreadyInUse;

        var passwordHash = passwordHasher.Hash(request.Password);

        var userResult = User.CreateAttendee(
            Guid.NewGuid(),
            request.PhoneNumber,
            request.Name,
            request.Email,
            passwordHash);

        if (userResult.IsError)
            return userResult.TopError;

        var user = userResult.Value;

        var rawCode = GenerateOtp();
        var codeHash = passwordHasher.Hash(rawCode);

        var verificationToken = VerificationToken.Create(
            Guid.NewGuid(),
            user.Id,
            VerificationTokenType.EmailConfirmation,
            codeHash,
            validFor: TimeSpan.FromHours(24));

        // Raise the event — will be dispatched by DbContext after SaveChangesAsync
        user.AddDomainEvent(new AttendeeRegisteredDomainEvent(user.Id, user.Email, rawCode));

        await db.Users.AddAsync(user, ct);
        await db.VerificationTokens.AddAsync(verificationToken, ct);

        // SaveChangesAsync commits, then dispatches AttendeeRegisteredDomainEvent
        // The email sends in the background — handler returns without waiting for it
        await db.SaveChangesAsync(ct);

        return Result.Success;
    }

    private static string GenerateOtp()
    {
        var randomNumber = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return randomNumber.ToString("D6");
    }
}