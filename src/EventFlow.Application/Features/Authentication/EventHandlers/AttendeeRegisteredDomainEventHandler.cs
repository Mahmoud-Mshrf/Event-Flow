using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Users.Events;
using MediatR;

namespace EventFlow.Application.Features.Authentication.EventHandlers;  
public sealed class AttendeeRegisteredDomainEventHandler(
    IEmailSender emailSender) : INotificationHandler<AttendeeRegisteredDomainEvent>
{
    public async Task Handle(AttendeeRegisteredDomainEvent notification, CancellationToken ct)
    {
        await emailSender.SendEmailConfirmationAsync(
            notification.Email,
            notification.RawCode,
            ct);
    }
}