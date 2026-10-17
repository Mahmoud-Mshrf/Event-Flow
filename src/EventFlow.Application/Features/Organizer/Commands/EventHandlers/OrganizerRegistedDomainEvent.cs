using EventFlow.Application.Common.Helpers;
using EventFlow.Application.Messages;
using EventFlow.Application.Settings;
using EventFlow.Domain.Users.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;

namespace EventFlow.Application.Features.Organizer.Commands.EventHandlers;

internal sealed class OrganizerRegisteredDomainEventHandler(
    IPublishEndpoint publishEndpoint,
    IOptions<VerificationTokenSettings> tokenSettings)
    : INotificationHandler<OrganizerRegisteredDomainEvent>
{
    public async Task Handle(
        OrganizerRegisteredDomainEvent notification,
        CancellationToken ct)
    {
        var hours = tokenSettings.Value.EmailConfirmationExpiryHours;

        await publishEndpoint.Publish(new EmailMessage
        {
            To = notification.Email,
            Subject = "Confirm your EventFlow organizer account",
            IsHtml = true,
            Body = $"""
                <h2>Welcome to EventFlow!</h2>
                <p>Your organizer account is almost ready.
                Confirm your email address with the code below:</p>
                {EmailHtml.Code(notification.RawCode)}
                <p>This code expires in {hours} hours.</p>
                <p style="color:#6b7280;font-size:13px;">
                    If you didn't create an account, you can ignore this email.
                </p>
                """
        }, ct);
    }
}