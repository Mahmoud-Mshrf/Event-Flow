using EventFlow.Domain.Common;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.StaffInvitations.Enums;
using EventFlow.Domain.Users.Enums;

namespace EventFlow.Domain.StaffInvitations;

public sealed class StaffInvitation : AuditableEntity
{
    // ── Properties ────────────────────────────────────────────
    public Guid TenantId { get; private set; }
    public Guid InvitedByUserId { get; private set; }
    public string InviteeEmail { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; private set; }
    public StaffInvitationStatus Status { get; private set; }
    public DateTime? AcceptedAtUtc { get; private set; }

    // ── EF Core constructor ───────────────────────────────────
    private StaffInvitation() { }

    // ── Private constructor ───────────────────────────────────
    private StaffInvitation(
        Guid id,
        Guid tenantId,
        Guid invitedByUserId,
        string inviteeEmail,
        UserRole role,
        string tokenHash,
        DateTime expiresAtUtc) : base(id)
    {
        TenantId = tenantId;
        InvitedByUserId = invitedByUserId;
        InviteeEmail = inviteeEmail;
        Role = role;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        Status = StaffInvitationStatus.Pending;
    }

    // ── Factory ───────────────────────────────────────────────
    // expiresAtUtc computed by handler using IDateTimeProvider
    // tokenHash computed by handler using IPasswordHasher
    // Domain receives final values — no hidden dependencies
    public static StaffInvitation Create(
        Guid id,
        Guid tenantId,
        Guid invitedByUserId,
        string inviteeEmail,
        UserRole role,
        string tokenHash,
        DateTime expiresAtUtc)
    {
        return new StaffInvitation(
            id,
            tenantId,
            invitedByUserId,
            inviteeEmail.ToLowerInvariant(),
            role,
            tokenHash,
            expiresAtUtc);
    }

    // ── Domain methods ────────────────────────────────────────
    // Token verification is done by the handler before calling this
    // Domain only handles state transitions — no infrastructure concerns
    public Result<Success> Accept(DateTime currentTime)
    {
        if (Status == StaffInvitationStatus.Accepted)
            return StaffInvitationErrors.AlreadyAccepted;

        if (Status == StaffInvitationStatus.Revoked)
            return StaffInvitationErrors.Revoked;

        if (currentTime > ExpiresAtUtc)
            return StaffInvitationErrors.Expired;

        Status = StaffInvitationStatus.Accepted;
        AcceptedAtUtc = currentTime;
        return Result.Success;
    }

    public Result<Success> Revoke()
    {
        if (Status != StaffInvitationStatus.Pending)
            return StaffInvitationErrors.CannotRevoke;

        Status = StaffInvitationStatus.Revoked;
        return Result.Success;
    }
}