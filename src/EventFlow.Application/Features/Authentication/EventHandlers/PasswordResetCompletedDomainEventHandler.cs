using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Users.Events;
using MediatR;

namespace EventFlow.Application.Features.Authentication.EventHandlers;

public sealed class PasswordResetCompletedDomainEventHandler(
    IEmailSender emailSender) : INotificationHandler<PasswordResetCompletedDomainEvent>
{
    public async Task Handle(
        PasswordResetCompletedDomainEvent notification, CancellationToken ct)
    {
        await emailSender.SendPasswordResetConfirmationAsync(
            notification.Email,
            ct);
    }
}