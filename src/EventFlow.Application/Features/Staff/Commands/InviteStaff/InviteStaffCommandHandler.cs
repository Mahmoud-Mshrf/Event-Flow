using EventFlow.Application.Common.Helpers;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.StaffInvitations;
using EventFlow.Domain.StaffInvitations.Enums;
using EventFlow.Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Staff.Commands.InviteStaff;

internal sealed class InviteStaffCommandHandler(
    IAppDbContext db,
    ICurrentTenant currentTenant,
    ICurrentUser currentUser,
    IPasswordHasher passwordHasher,
    IEmailSender emailSender,IDateTimeProvider dateTimeProvider) : IRequestHandler<InviteStaffCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(
        InviteStaffCommand request,
        CancellationToken ct)
    {
        var tenantId = currentTenant.TenantGuid!.Value;
        var email = request.InviteeEmail.ToLowerInvariant();

        // ── 1. Check email not already registered ─────────────────────
        // One email = one User record — staff and attendee cannot share email
        var emailTaken = await db.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email == email, ct);

        if (emailTaken)
            return UserErrors.EmailAlreadyInUse;

        // ── 2. Check no pending invitation already exists ─────────────
        // DB unique index enforces this too, but we check first for
        // a clean error message instead of catching a DB exception
        var pendingExists = await db.StaffInvitations
            .AnyAsync(i => i.InviteeEmail == email
                        && i.Status == StaffInvitationStatus.Pending, ct);

        if (pendingExists)
            return StaffInvitationErrors.PendingInvitationExists;

        // ── 3. Generate token ─────────────────────────────────────────
        var rawToken =OtpGenerator.Generate();
        var tokenHash = passwordHasher.Hash(rawToken);

        // ── 4. Compute expiry using IDateTimeProvider ─────────────────
        // Domain receives absolute DateTime — not a TimeSpan
        var expiresAtUtc = dateTimeProvider.UtcNow.Add(TimeSpan.FromHours(48));

        // ── 5. Create invitation ──────────────────────────────────────
        var invitation = StaffInvitation.Create(
            id: Guid.NewGuid(),
            tenantId: tenantId,
            invitedByUserId: currentUser.UserId!,
            inviteeEmail: email,
            role: request.Role,
            tokenHash: tokenHash,
            expiresAtUtc: expiresAtUtc);

        await db.StaffInvitations.AddAsync(invitation, ct);
        await db.SaveChangesAsync(ct);

        // ── 5. Send invitation email ──────────────────────────────────
        // Raw token goes in the email link — never stored in DB
        var subject = "You've been invited to join EventFlow";
        var body = $"""
            You have been invited to join an organization on EventFlow 
            as an {request.Role}.

            Use the token below to accept your invitation:

            Token: {rawToken}

            This invitation expires in 48 hours.

            If you did not expect this invitation, you can safely ignore this email.
            """;

        await emailSender.SendAsync(email, subject, body,ct);

        return Result.Success;
    }
}