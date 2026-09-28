using EventFlow.Application.Common.Helpers;

namespace EventFlow.Application.Common.Interfaces;

public interface IEmailSender
{
    Task SendEmailConfirmationAsync(string toEmail, string code, CancellationToken ct);
    Task SendPasswordResetOtpAsync(string toEmail, string code, CancellationToken ct);
    Task SendPasswordResetConfirmationAsync(string toEmail, CancellationToken ct);

    // New:
    Task SendTicketConfirmationAsync(
        string toEmail,
        string attendeeName,
        string eventName,
        string orderNumber,
        IReadOnlyCollection<IssuedTicketInfo> tickets,
        CancellationToken ct);
}
