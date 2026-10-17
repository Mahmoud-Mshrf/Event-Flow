using EventFlow.Application.Common.Helpers;
using EventFlow.Application.Messages;
using EventFlow.Domain.Users.Events;
using MassTransit;
using MediatR;

namespace EventFlow.Application.Features.Authentication.EventHandlers;

internal sealed class PasswordResetCompletedDomainEventHandler(
    IPublishEndpoint publishEndpoint)
    : INotificationHandler<PasswordResetCompletedDomainEvent>
{
    public async Task Handle(
        PasswordResetCompletedDomainEvent notification,
        CancellationToken ct)
    {
        await publishEndpoint.Publish(new EmailMessage
        {
            To = notification.Email,
            Subject = "Your EventFlow password has been reset",
            IsHtml = true,
            Body = $"""
                <h2>Password changed</h2>
                <p>Hi {EmailHtml.Encode(notification.Name)},</p>
                <p>Your password was reset successfully, and you've been
                signed out of all devices.</p>
                <p style="color:#b91c1c;">
                    If this wasn't you, contact support immediately.
                </p>
                """
        }, ct);
    }
}