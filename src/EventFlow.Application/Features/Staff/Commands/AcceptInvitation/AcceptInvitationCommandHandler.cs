using EventFlow.Application.Common.Helpers;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Messages;
using EventFlow.Application.Settings;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.StaffInvitations;
using EventFlow.Domain.StaffInvitations.Enums;
using EventFlow.Domain.Users;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;

namespace EventFlow.Application.Features.Staff.Commands.AcceptInvitation;

internal sealed class AcceptInvitationCommandHandler(
    IAppDbContext db,
    IPasswordHasher passwordHasher,
    IDateTimeProvider dateTimeProvider,
    IEmailSender emailSender,HybridCache cache,
    IOptions<FrontendSettings> frontendSettings,IPublishEndpoint publishEndpoint)
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
        await cache.RemoveByTagAsync($"tenant-{invitation.TenantId}-staff", ct);
        await cache.RemoveByTagAsync($"tenant-{invitation.TenantId}-invitations", ct);
        // ── 6. Welcome email ──────────────────────────────────────────
        await publishEndpoint.Publish(new EmailMessage
        {
            To = email,
            Subject = "Welcome to EventFlow!",
            IsHtml = true,
            Body = $"""
                <h2>Welcome to the team!</h2>
                <p>Hi {EmailHtml.Encode(request.Name)},</p>
                <p>Your account has been created. You can log in with this email
                and the password you just set.</p>
                {EmailHtml.Button($"{frontendSettings.Value.BaseUrl}/login", "Log in")}
                """
        }, ct);

        return Result.Success;
    }
}