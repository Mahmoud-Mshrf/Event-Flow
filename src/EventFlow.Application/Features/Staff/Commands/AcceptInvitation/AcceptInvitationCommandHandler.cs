using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.StaffInvitations;
using EventFlow.Domain.StaffInvitations.Enums;
using EventFlow.Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Staff.Commands.AcceptInvitation;

internal sealed class AcceptInvitationCommandHandler(
    IAppDbContext db,
    IPasswordHasher passwordHasher,
    IDateTimeProvider dateTimeProvider,
    IEmailSender emailSender)
    : IRequestHandler<AcceptInvitationCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(
        AcceptInvitationCommand request,
        CancellationToken ct)
    {
        var email = request.InviteeEmail.ToLowerInvariant();
        var now = dateTimeProvider.UtcNow;

        // ── 1. Find the pending invitation ────────────────────────────
        // IgnoreQueryFilters — no tenant context when accepting
        var invitation = await db.StaffInvitations
        .IgnoreQueryFilters()
        .Where(i => i.InviteeEmail == email
                && i.Status == StaffInvitationStatus.Pending)
        .OrderByDescending(i => i.CreatedAtUtc)
        .FirstOrDefaultAsync(ct);

        if (invitation is null)
            return StaffInvitationErrors.NotFound;

        // ── 2. Verify token — Application layer concern ───────────────
        // Domain's Accept() handles state transitions only
        // Hash verification stays here, not in the domain entity
        var isValidToken = passwordHasher.Verify(
            request.Token,
            invitation.TokenHash);

        if (!isValidToken)
            return StaffInvitationErrors.InvalidToken;

        // ── 3. Apply state transition ─────────────────────────────────
        // Accept() checks status (Pending/Accepted/Revoked) and expiry
        var acceptResult = invitation.Accept(now);
        if (acceptResult.IsError)
            return acceptResult.Errors!;

        // ── 4. Race condition guard ───────────────────────────────────
        // Two concurrent requests with same token could both pass step 2
        // This check catches the second one after the first has saved
        var emailTaken = await db.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email == email, ct);

        if (emailTaken)
            return UserErrors.EmailAlreadyInUse;

        // ── 5. Create staff User ──────────────────────────────────────
        // Email is pre-confirmed — clicking the invitation link
        // proves they control this email address
        var passwordHash = passwordHasher.Hash(request.Password);

        var userResult = User.CreateStaff(
            id: Guid.NewGuid(),
            name: request.Name,
            email: email,
            passwordHash: passwordHash,
            phoneNumber: string.Empty,
            role: invitation.Role,
            tenantId: invitation.TenantId);

        if (userResult.IsError)
            return userResult.Errors!;

        var user = userResult.Value;

        // Email confirmed automatically — no separate confirmation needed
        user.ConfirmEmail();

        await db.Users.AddAsync(user, ct);
        await db.SaveChangesAsync(ct);

        // ── 6. Welcome email ──────────────────────────────────────────
        var subject = "Welcome to EventFlow!";
        var body = $"""
            Dear {request.Name},

            Your account has been successfully created.
            You can now log in using your email and the password
            you set during invitation acceptance.

            Welcome to the team!
            """;

        await emailSender.SendAsync(email, subject, body,ct);

        return Result.Success;
    }
}