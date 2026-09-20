namespace EventFlow.Application.Common.Interfaces;

public interface IEmailSender
{
    Task SendEmailConfirmationAsync(string toEmail, string code, CancellationToken ct);
    Task SendPasswordResetOtpAsync(string toEmail, string code, CancellationToken ct);
    Task SendPasswordResetConfirmationAsync(string toEmail, CancellationToken ct); // ← add
}

