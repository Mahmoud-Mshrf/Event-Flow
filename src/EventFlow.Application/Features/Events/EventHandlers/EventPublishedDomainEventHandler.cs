using EventFlow.Application.Common.Helpers;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Messages;
using EventFlow.Domain.Events.DomainEvents;
using EventFlow.Domain.Events.Enums;
using EventFlow.Domain.Users.Enums;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Events.EventHandlers;

internal sealed class EventPublishedDomainEventHandler(
    IAppDbContext db,
    IPublishEndpoint publishEndpoint)
    : INotificationHandler<EventPublishedDomainEvent>
{
    public async Task Handle(
        EventPublishedDomainEvent notification,
        CancellationToken ct)
    {
        // User has no global filter, so tenant scoping is explicit
        var owners = await db.Users
            .AsNoTracking()
            .Where(u => u.TenantId == notification.TenantId
                     && u.Role == UserRole.Owner
                     && !u.Disabled)
            .Select(u => new { u.Email, u.Name })
            .ToListAsync(ct);

        if (owners.Count == 0)
            return;

        var eventName = EmailHtml.Encode(notification.EventName);

        var visibilityParagraph = notification.Visibility == EventVisibility.Public
            ? "<p>It is <strong>publicly visible</strong>. Attendees can discover " +
              "it and register right away.</p>"
            : "<p>It is a <strong>private</strong> event and won't appear in public " +
              "search results. Share the direct link with your intended attendees.</p>";

        foreach (var owner in owners)
        {
            await publishEndpoint.Publish(new EmailMessage
            {
                To = owner.Email,
                Subject = $"Your event \"{notification.EventName}\" is now live!",
                IsHtml = true,
                Body = $"""
                    <h2>Your event is live!</h2>
                    <p>Hi {EmailHtml.Encode(owner.Name)},</p>
                    <p><strong>{eventName}</strong> has been published.</p>
                    {visibilityParagraph}
                    <p>Good luck with your event!</p>
                    """
            }, ct);
        }
    }
}