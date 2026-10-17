using EventFlow.Application.Common.Helpers;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Messages;
using EventFlow.Application.Settings;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.StaffInvitations;
using EventFlow.Domain.StaffInvitations.Enums;
using EventFlow.Domain.Users;
using MassTransit.Transports;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;

namespace EventFlow.Application.Features.Staff.Commands.InviteStaff;

internal sealed class InviteStaffCommandHandler(
    IAppDbContext db,
    ICurrentTenant currentTenant,
    ICurrentUser currentUser,
    IPasswordHasher passwordHasher,
    IDateTimeProvider dateTimeProvider,
    IEmailSender emailSender,
    IOptions<FrontendSettings> frontendSettings,HybridCache cache,PublishEndpoint publishEndpoint)
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
        await cache.RemoveByTagAsync($"tenant-{tenantId}-invitations", ct);
        // ── 6. Build the clickable link ───────────────────────────────
        var baseUrl = frontendSettings.Value.BaseUrl;
        var invitationLink =
            $"{baseUrl}/accept-invitation" +
            $"?token={Uri.EscapeDataString(rawToken)}" +
            $"&email={Uri.EscapeDataString(email)}";

        var tenantName = await db.Tenants
            .Where(t => t.Id == tenantId)
            .Select(t => t.Name)
            .FirstOrDefaultAsync(ct) ?? "an organization";

        // ── 7. Publish the invitation email ───────────────────────────
        await publishEndpoint.Publish(new EmailMessage
        {
            To = email,
            Subject = $"You've been invited to join {tenantName} on EventFlow",
            IsHtml = true,
            Body = $"""
                <h2>You're invited!</h2>
                <p>You've been invited to join <strong>{EmailHtml.Encode(tenantName)}</strong>
                on EventFlow as <strong>{request.Role}</strong>.</p>
                <p>Click the button below to set up your account.</p>
                {EmailHtml.Button(invitationLink, "Accept invitation")}
                <p>This invitation expires in 48 hours.</p>
                <p style="color:#6b7280;font-size:13px;">
                    If you weren't expecting this, you can ignore this email.
                </p>
                """
        }, ct);

        return Result.Success;
    }
}