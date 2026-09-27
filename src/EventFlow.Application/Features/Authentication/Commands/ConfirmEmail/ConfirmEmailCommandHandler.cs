using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Identity;
using EventFlow.Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Authentication.Commands.ConfirmEmail;  

public sealed class ConfirmEmailCommandHandler(
    IAppDbContext db,
    IPasswordHasher passwordHasher) : IRequestHandler<ConfirmEmailCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(ConfirmEmailCommand request, CancellationToken ct)
    {
        // 1. Find the user by email
        var user = await db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == request.Email, ct);

        if (user is null)
            return UserErrors.NotFound;

        // 2. Guard: already confirmed — no need to go further
        if (user.EmailConfirmed)
            return UserErrors.EmailAlreadyConfirmed;

        // 3. Find the most recent unused EmailConfirmation token for this user
        var token = await db.VerificationTokens
            .Where(t => t.UserId == user.Id
                && t.Type == VerificationTokenType.EmailConfirmation
                && !t.Used)
            .OrderByDescending(t => t.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);

        if (token is null)
            return VerificationTokenErrors.NotFound;

        // 4. Verify the raw code against the stored hash — infrastructure concern, lives here
        if (!passwordHasher.Verify(request.Code, token.CodeHash))
            return VerificationTokenErrors.InvalidCode;

        // 5. Consume the token — domain rules (used? expired?) enforced by the entity
        var consumeResult = token.Consume(DateTime.UtcNow);
        if (consumeResult.IsError)
            return consumeResult.TopError;

        // 6. Confirm the user's email — domain rule enforced by the entity
        var confirmResult = user.ConfirmEmail();
        if (confirmResult.IsError)
            return confirmResult.TopError;

        await db.SaveChangesAsync(ct);

        return Result.Success;
    }
}