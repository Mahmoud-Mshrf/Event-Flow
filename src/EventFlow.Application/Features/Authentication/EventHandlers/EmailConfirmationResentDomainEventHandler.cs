using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Users.Events;
using MediatR;

namespace EventFlow.Application.Features.Authentication.EventHandlers;

public sealed class EmailConfirmationResentDomainEventHandler(
    IEmailSender emailSender) : INotificationHandler<EmailConfirmationResentDomainEvent>
{
    public async Task Handle(
        EmailConfirmationResentDomainEvent notification, CancellationToken ct)
    {
        await emailSender.SendEmailConfirmationAsync(
            notification.Email,
            notification.RawCode,
            ct);
    }
}