using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Users.Events;
using MediatR;

namespace EventFlow.Application.Features.Authentication.EventHandlers;

public sealed class PasswordResetRequestedDomainEventHandler(
    IEmailSender emailSender) : INotificationHandler<PasswordResetRequestedDomainEvent>
{
    public async Task Handle(
        PasswordResetRequestedDomainEvent notification, CancellationToken ct)
    {
        await emailSender.SendPasswordResetOtpAsync(
            notification.Email,
            notification.RawCode,
            ct);
    }
}