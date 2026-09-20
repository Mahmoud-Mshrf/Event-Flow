using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Users.Events;
using MediatR;

namespace eventflow.application.features.organizer.commands.registerorganizer;

public sealed class OrganizerRegisteredDomainEventHandler(
    IEmailSender emailSender) : INotificationHandler<OrganizerRegisteredDomainEvent>
{
    public async Task Handle(
        OrganizerRegisteredDomainEvent notification, CancellationToken ct)
    {
        await emailSender.SendEmailConfirmationAsync(
            notification.Email,
            notification.RawCode,
            ct);
    }
}