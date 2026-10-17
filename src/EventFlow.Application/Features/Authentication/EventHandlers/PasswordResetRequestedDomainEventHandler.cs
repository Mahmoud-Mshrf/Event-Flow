using EventFlow.Application.Common.Helpers;
using EventFlow.Application.Messages;
using EventFlow.Application.Settings;
using EventFlow.Domain.Users.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;

namespace EventFlow.Application.Features.Authentication.EventHandlers;

internal sealed class PasswordResetRequestedDomainEventHandler(
    IPublishEndpoint publishEndpoint,
    IOptions<VerificationTokenSettings> tokenSettings)
    : INotificationHandler<PasswordResetRequestedDomainEvent>
{
    public async Task Handle(
        PasswordResetRequestedDomainEvent notification,
        CancellationToken ct)
    {
        var minutes = tokenSettings.Value.PasswordResetExpiryMinutes;

        await publishEndpoint.Publish(new EmailMessage
        {
            To = notification.Email,
            Subject = "Reset your EventFlow password",
            IsHtml = true,
            Body = $"""
                <h2>Reset your password</h2>
                <p>We received a request to reset your password.
                Use the code below:</p>
                {EmailHtml.Code(notification.RawCode)}
                <p>This code expires in {minutes} minutes.</p>
                <p style="color:#6b7280;font-size:13px;">
                    If you didn't request this, you can ignore this email.
                    Your password won't change.
                </p>
                """
        }, ct);
    }
}