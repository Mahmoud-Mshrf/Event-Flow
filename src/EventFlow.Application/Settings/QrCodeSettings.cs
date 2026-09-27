namespace EventFlow.Application.Settings;
public sealed class QrCodeSettings
{
    // Long random secret — treat like a password
    // If this leaks, anyone can forge QR codes
    // Minimum 32 characters
    public string Secret { get; init; } = null!;
}