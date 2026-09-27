using EventFlow.Domain.Common;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Users;

namespace EventFlow.Domain.Identity;

// public class VerificationToken : AuditableEntity
// {
//     public Guid UserId { get; private set; }
//     public VerificationTokenType Type { get; private set; }
//     public string CodeHash { get; private set; } = null!;   // never store the raw code/token
//     public DateTime ExpiresAtUtc { get; private set; }
//     public bool Used { get; private set; }

//     private VerificationToken() { }

//     private VerificationToken(Guid id, Guid userId, VerificationTokenType type, string codeHash, DateTime expiresAtUtc)
//         : base(id)
//     {
//         UserId = userId;
//         Type = type;
//         CodeHash = codeHash;
//         ExpiresAtUtc = expiresAtUtc;
//     }

//     public static VerificationToken Create(Guid id, Guid userId, VerificationTokenType type, string codeHash, TimeSpan validFor) =>
//         new(id, userId, type, codeHash, DateTime.UtcNow.Add(validFor));

//     public Result<Success> Consume(string providedCode, IPasswordHasher hasher, DateTime nowUtc)
//     {
//         if (Used)
//             return VerificationTokenErrors.AlreadyUsed;

//         if (nowUtc > ExpiresAtUtc)
//             return VerificationTokenErrors.Expired;

//         if (!hasher.Verify(providedCode, CodeHash))
//             return VerificationTokenErrors.InvalidCode;

//         Used = true;
//         return Result.Success;
//     }
// }

public class VerificationToken : AuditableEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public VerificationTokenType Type { get; private set; }
    public string CodeHash { get; private set; } = null!;
    public DateTime ExpiresAtUtc { get; private set; }
    public bool Used { get; private set; }

    private VerificationToken() { }

    private VerificationToken(
        Guid id,
        Guid userId,
        VerificationTokenType type,
        string codeHash,
        DateTime expiresAtUtc)
        : base(id)
    {
        UserId = userId;
        Type = type;
        CodeHash = codeHash;
        ExpiresAtUtc = expiresAtUtc;
    }

    public static VerificationToken Create(
        Guid id,
        Guid userId,
        VerificationTokenType type,
        string codeHash,
        TimeSpan validFor)
        => new(
            id,
            userId,
            type,
            codeHash,
            DateTime.UtcNow.Add(validFor));

    public Result<Success> Consume(DateTime nowUtc)
    {
        if (Used)
            return VerificationTokenErrors.AlreadyUsed;

        if (nowUtc > ExpiresAtUtc)
            return VerificationTokenErrors.Expired;

        Used = true;

        return Result.Success;
    }
}