using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.CheckIn.Dtos;
using EventFlow.Domain.Tickets.Enums;
using EventFlow.Domain.Tickets.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.CheckIn.DomainEventHandlers;

// EventFlow.Application/Features/CheckIn/DomainEventHandlers/AttendeeCheckedInDomainEventHandler.cs
public sealed class AttendeeCheckedInDomainEventHandler(
    IEventDashboardNotifier notifier,
    IAppDbContext db)
    : INotificationHandler<AttendeeCheckedInDomainEvent>
{
    public async Task Handle(
        AttendeeCheckedInDomainEvent notification,
        CancellationToken ct)
    {
        // 1. Compute fresh stats from DB — the source of truth
        //    Don't try to "increment" a cached number — that causes drift
        //    Just recount from scratch — it's one query, it's fast
        var stats = await db.Tickets
            .IgnoreQueryFilters()  // no tenant context in a domain event handler
            .Where(t => t.EventId == notification.EventId)
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var total = stats.Sum(s => s.Count);
        var checkedIn = stats
            .FirstOrDefault(s => s.Status == TicketStatus.CheckedIn)?.Count ?? 0;

        var dashboardUpdate = new EventDashboardUpdate
        (
            notification.EventId,
            total,
            checkedIn,
            total - checkedIn,
            notification.CheckedInAt
        );

        // 2. Push to all clients in this event's group
        //    IHubContext lets you push from outside the Hub class
        //    (from domain event handlers, background jobs, etc.)
        //    "DashboardUpdated" is the method name the frontend listens for
        await notifier.NotifyDashboardUpdatedAsync(dashboardUpdate);
    }
}