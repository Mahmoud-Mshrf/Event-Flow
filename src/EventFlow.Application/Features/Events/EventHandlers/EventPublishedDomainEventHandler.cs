using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Events.DomainEvents;
using EventFlow.Domain.Events.Enums;
using EventFlow.Domain.Users.Enums;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Events.EventHandlers;

internal sealed class EventPublishedDomainEventHandler(
    IAppDbContext db,
    IEmailSender emailSender)
    : INotificationHandler<EventPublishedDomainEvent>
{
    public async Task Handle(
        EventPublishedDomainEvent notification,
        CancellationToken ct)
    {
        // Load the tenant Owners — they're the one who published the event
        // IgnoreQueryFilters because User has no global filter
        var owners = await db.Users
        .Where(u => u.TenantId == notification.TenantId
                && u.Role == UserRole.Owner)
        .Select(u => new { u.Email, u.Name })
        .ToListAsync(ct);

        foreach(var owner in owners)
        {
            var subject = $"Your event \"{notification.EventName}\" is now live!";

            var body = notification.Visibility == EventVisibility.Public
                ? $"""
                Dear {owner.Name},

                Great news! Your event "{notification.EventName}" has been 
                successfully published and is now publicly visible.

                Attendees can discover and register for your event immediately.

                Good luck with your event!
                """
                : $"""
                Dear {owner.Name},

                Your event "{notification.EventName}" has been successfully 
                published as a private event.

                It won't appear in public search results — share the direct 
                link with your intended attendees to give them access.

                Good luck with your event!
                """;

            await emailSender.SendAsync(owner.Email, subject, body,false,ct);
        }
    }
}