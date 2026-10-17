using EventFlow.Application.Common.Helpers;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Settings;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.StaffInvitations;
using EventFlow.Domain.StaffInvitations.Enums;
using EventFlow.Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EventFlow.Application.Features.Staff.Commands.InviteStaff;

internal sealed class InviteStaffCommandHandler(
    IAppDbContext db,
    ICurrentTenant currentTenant,
    ICurrentUser currentUser,
    IPasswordHasher passwordHasher,
    IDateTimeProvider dateTimeProvider,
    IEmailSender emailSender,
    IOptions<FrontendSettings> frontendSettings)
    : IRequestHandler<InviteStaffCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(
        InviteStaffCommand request,
        CancellationToken ct)
    {
        var tenantId = currentTenant.TenantGuid!.Value;
        var email = request.InviteeEmail.ToLowerInvariant();

        // ── 1. Check email not already registered ─────────────────────
        var emailTaken = await db.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email == email, ct);

        if (emailTaken)
            return UserErrors.EmailAlreadyInUse;

        // ── 2. Check no pending invitation already exists ─────────────
        var pendingExists = await db.StaffInvitations
            .AnyAsync(i => i.InviteeEmail == email
                        && i.Status == StaffInvitationStatus.Pending, ct);

        if (pendingExists)
            return StaffInvitationErrors.PendingInvitationExists;

        // ── 3. Generate secure token ──────────────────────────────────
        // Long-lived token (48 hrs) needs cryptographic strength
        // raw token goes in the email link, hash goes in DB
        var rawToken = OtpGenerator.GenerateSecureToken();
        var tokenHash = passwordHasher.Hash(rawToken);

        // ── 4. Compute expiry using IDateTimeProvider ─────────────────
        var expiresAtUtc = dateTimeProvider.UtcNow
            .Add(TimeSpan.FromHours(48));

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

        // ── 6. Build clickable invitation link ────────────────────────
        // Frontend reads token + email from URL params
        // Pre-fills them in the form — user only types name + password
        var baseUrl = frontendSettings.Value.BaseUrl;
        var invitationLink =
            $"{baseUrl}/accept-invitation" +
            $"?token={Uri.EscapeDataString(rawToken)}" +
            $"&email={Uri.EscapeDataString(email)}";

        // ── 7. Send invitation email ──────────────────────────────────
        var subject = "You've been invited to join EventFlow";
        var body = $"""
            You have been invited to join an organization on EventFlow
            as an {request.Role}.

            Click the link below to accept your invitation and set up your account:

            {invitationLink}

            This invitation expires in 48 hours.

            If you did not expect this invitation, you can safely ignore this email.
            """;

        await emailSender.SendAsync(email, subject, body,ct);

        return Result.Success;
    }
}