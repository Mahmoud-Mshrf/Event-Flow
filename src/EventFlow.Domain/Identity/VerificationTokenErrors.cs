using EventFlow.Domain.Common.Errors;

namespace EventFlow.Domain.Identity;
public static class VerificationTokenErrors
{
    public static readonly Error AlreadyUsed =
        Error.Conflict(
            "VerificationToken.AlreadyUsed",
            "The verification token has already been used.");

    public static readonly Error Expired =
        Error.Conflict(
            "VerificationToken.Expired",
            "The verification token has expired.");

    public static readonly Error InvalidCode =
        Error.Validation(
            "VerificationToken.InvalidCode",
            "The verification code is invalid.");
}