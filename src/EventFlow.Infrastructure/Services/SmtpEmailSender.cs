using EventFlow.Application.Common.Helpers;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Infrastructure.Helpers;

namespace EventFlow.Infrastructure.Services;
public sealed class SmtpEmailSender(EmailService emailService) : IEmailSender
{
    public async Task SendEmailConfirmationAsync(
        string toEmail, string code, CancellationToken ct)
    {
        var message = new EmailMessage
        {
            To = toEmail,
            Subject = "Confirm your EventFlow account",
            IsHtml = true,
            Body = $"""
                <h2>Welcome to EventFlow!</h2>
                <p>Use the code below to confirm your email address.</p>
                <p>This code expires in <strong>24 hours</strong>.</p>
                <div style="
                    display: inline-block;
                    padding: 12px 24px;
                    background-color: #1a1a2e;
                    color: #ffffff;
                    font-size: 32px;
                    font-weight: bold;
                    letter-spacing: 8px;
                    border-radius: 8px;
                    margin: 16px 0;">
                    {code}
                </div>
                <p>If you did not create an EventFlow account, you can safely ignore this email.</p>
                """
        };

        await emailService.SendEmailAsync(message);
    }

    public async Task SendPasswordResetOtpAsync(
        string toEmail, string code, CancellationToken ct)
    {
        var message = new EmailMessage
        {
            To = toEmail,
            Subject = "Reset your EventFlow password",
            IsHtml = true,
            Body = $"""
                <h2>Password Reset Request</h2>
                <p>Use the code below to reset your EventFlow password.</p>
                <p>This code expires in <strong>15 minutes</strong>.</p>
                <div style="
                    display: inline-block;
                    padding: 12px 24px;
                    background-color: #1a1a2e;
                    color: #ffffff;
                    font-size: 32px;
                    font-weight: bold;
                    letter-spacing: 8px;
                    border-radius: 8px;
                    margin: 16px 0;">
                    {code}
                </div>
                <p>If you did not request a password reset, please secure your account immediately.</p>
                <p>This code can only be used once.</p>
                """
        };

        await emailService.SendEmailAsync(message);
    }

    public async Task SendPasswordResetConfirmationAsync(
        string toEmail, CancellationToken ct)
    {
        var message = new EmailMessage
        {
            To = toEmail,
            Subject = "Your EventFlow password was changed",
            IsHtml = true,
            Body = """
                <h2>Password Changed Successfully</h2>
                <p>Your EventFlow password was just reset.</p>
                <p>All active sessions have been signed out for your security.</p>
                <p>If you did not make this change, please contact support immediately
                   or request another password reset to secure your account.</p>
                """
        };

        await emailService.SendEmailAsync(message);
    }
    public async Task SendTicketConfirmationAsync(
        string toEmail,
        string attendeeName,
        string eventName,
        string orderNumber,
        IReadOnlyCollection<IssuedTicketInfo> tickets,
        CancellationToken ct)
    {
        var ticketRows = string.Join("", tickets.Select(t => $"""
            <tr>
                <td style="padding:8px;border:1px solid #ddd;">{t.TicketNumber}</td>
                <td style="padding:8px;border:1px solid #ddd;">{t.TicketTypeName}</td>
            </tr>
            """));

        var message = new EmailMessage
        {
            To = toEmail,
            Subject = $"Your tickets for {eventName}",
            IsHtml = true,
            Body = $"""
                <h2>Your tickets are confirmed!</h2>
                <p>Hi {attendeeName},</p>
                <p>Thank you for your purchase. Your tickets for <strong>{eventName}</strong> are ready.</p>
                <p><strong>Order Number:</strong> {orderNumber}</p>

                <table style="border-collapse:collapse;width:100%;margin:16px 0;">
                    <thead>
                        <tr style="background:#1a1a2e;color:white;">
                            <th style="padding:8px;text-align:left;">Ticket Number</th>
                            <th style="padding:8px;text-align:left;">Type</th>
                        </tr>
                    </thead>
                    <tbody>{ticketRows}</tbody>
                </table>

                <p>Present your ticket QR code at the event entrance.
                You can view your tickets anytime in the EventFlow app.</p>
                """
        };

        await emailService.SendEmailAsync(message, ct);
    }
}
