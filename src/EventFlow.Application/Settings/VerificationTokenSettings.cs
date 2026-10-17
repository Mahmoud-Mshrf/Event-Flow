namespace EventFlow.Application.Settings;

public sealed class VerificationTokenSettings
{
    public const string SectionName = "VerificationTokens";
    public int EmailConfirmationExpiryHours { get; init; } = 24;
    public int PasswordResetExpiryMinutes { get; init; } = 15;
}
