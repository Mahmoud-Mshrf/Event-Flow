using EventFlow.Application.Common.Helpers;
using EventFlow.Application.Messages;
using EventFlow.Application.Settings;
using EventFlow.Domain.Users.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;

namespace EventFlow.Application.Features.Authentication.EventHandlers;

internal sealed class AttendeeRegisteredDomainEventHandler(
    IPublishEndpoint publishEndpoint,
    IOptions<VerificationTokenSettings> tokenSettings)
    : INotificationHandler<AttendeeRegisteredDomainEvent>
{
    public async Task Handle(
        AttendeeRegisteredDomainEvent notification,
        CancellationToken ct)
    {
        var hours = tokenSettings.Value.EmailConfirmationExpiryHours;

        await publishEndpoint.Publish(new EmailMessage
        {
            To = notification.Email,
            Subject = "Confirm your EventFlow account",
            IsHtml = true,
            Body = $"""
                <h2>Welcome to EventFlow!</h2>
                <p>Please confirm your email address with the code below:</p>
                {EmailHtml.Code(notification.RawCode)}
                <p>This code expires in {hours} hours.</p>
                <p style="color:#6b7280;font-size:13px;">
                    If you didn't create an account, you can ignore this email.
                </p>
                """
        }, ct);
    }
}