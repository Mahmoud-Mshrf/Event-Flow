using EventFlow.Domain.Common;
using EventFlow.Domain.Common.Errors;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Users;

namespace EventFlow.Domain.Identity;
public class RefreshToken : AuditableEntity
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }
    // On RefreshToken entity — add if missing:
    public User User { get; private set; } = null!;
    public bool IsActive => RevokedAtUtc is null && DateTime.UtcNow < ExpiresAtUtc;

    private RefreshToken() { } // EF Core

    private RefreshToken(Guid id, Guid userId, string tokenHash, DateTime expiresAtUtc)
        : base(id)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
    }

    public static RefreshToken Issue(Guid id, Guid userId, string tokenHash, TimeSpan validFor) =>
        new(id, userId, tokenHash, DateTime.UtcNow.Add(validFor));

    public Result<Success> Revoke(Guid? replacedByTokenId = null)
    {
        if (!IsActive)
            return RefreshTokenErrors.AlreadyInactive;

        RevokedAtUtc = DateTime.UtcNow;
        ReplacedByTokenId = replacedByTokenId;

        return Result.Success;
    }
}

public static class RefreshTokenErrors
{
    public static readonly Error AlreadyInactive =
        Error.Validation("RefreshToken.AlreadyInactive", "This refresh token has already been revoked or has expired.");

    public static readonly Error NotFoundOrInactive =
        Error.Validation("RefreshToken.NotFoundOrInactive", "Invalid or expired refresh token.");
}