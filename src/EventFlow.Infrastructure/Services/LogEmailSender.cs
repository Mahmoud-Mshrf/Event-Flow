using EventFlow.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace EventFlow.Infrastructure.Services;

public sealed class LogEmailSender(ILogger<LogEmailSender> logger) : IEmailSender
{
    public Task SendEmailConfirmationAsync(string toEmail, string code, CancellationToken ct)
    {
        logger.LogInformation(
            "[EMAIL] Confirmation code for {Email}: {Code}",
            toEmail, code);

        return Task.CompletedTask;
    }

    public Task SendPasswordResetOtpAsync(string toEmail, string code, CancellationToken ct)
    {
        logger.LogInformation(
            "[EMAIL] Password reset OTP for {Email}: {Code}",
            toEmail, code);

        return Task.CompletedTask;
    }

    public Task SendPasswordResetConfirmationAsync(string toEmail, CancellationToken ct)
    {
        logger.LogInformation(
            "[EMAIL] Password reset confirmation sent to {Email}",
            toEmail);

        return Task.CompletedTask;
    }
}