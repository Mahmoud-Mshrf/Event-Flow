namespace EventFlow.Application.Common.Interfaces;

public interface IEmailSender
{
    Task SendEmailConfirmationAsync(string toEmail, string confirmationCode, CancellationToken ct);
    Task SendPasswordResetOtpAsync(string toEmail, string otpCode, CancellationToken ct);
}