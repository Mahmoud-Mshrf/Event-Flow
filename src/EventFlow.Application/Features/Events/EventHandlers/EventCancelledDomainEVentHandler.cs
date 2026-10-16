using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Messages;
using EventFlow.Domain.Events.DomainEvents;
using EventFlow.Domain.Orders.Enums;
using EventFlow.Domain.Payments.Enums;
using EventFlow.Domain.Tickets.Enums;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Events.EventHandlers;

internal sealed class EventCancelledDomainEventHandler(
    IAppDbContext db,
    IPublishEndpoint publishEndpoint)
    : INotificationHandler<EventCancelledDomainEvent>
{
    public async Task Handle(
        EventCancelledDomainEvent notification,
        CancellationToken ct)
    {
        // ── 1. Handle Pending orders ──────────────────────────────────
        var pendingOrders = await db.Orders
            .IgnoreQueryFilters()
            .Include(o => o.OrderItems)
            .Where(o => o.EventId == notification.EventId
                     && o.TenantId == notification.TenantId
                     && o.OrderStatus == OrderStatus.Pending)
            .ToListAsync(ct);

        if (pendingOrders.Count > 0)
        {
            var ticketTypeIds = pendingOrders
                .SelectMany(o => o.OrderItems)
                .Select(i => i.TicketTypeId)
                .Distinct()
                .ToList();

            var ticketTypes = await db.TicketTypes
                .IgnoreQueryFilters()
                .Where(tt => ticketTypeIds.Contains(tt.Id))
                .ToListAsync(ct);

            foreach (var order in pendingOrders)
            {
                order.Cancel();

                foreach (var item in order.OrderItems)
                {
                    var ticketType = ticketTypes
                        .FirstOrDefault(tt => tt.Id == item.TicketTypeId);

                    ticketType?.Release(item.Quantity);
                }
            }

            // ── 2. Mark stale Pending payments as Failed ──────────────
            var pendingOrderIds = pendingOrders
                .Select(o => o.Id)
                .ToList();

            var pendingPayments = await db.Payments
                .IgnoreQueryFilters()
                .Where(p => pendingOrderIds.Contains(p.OrderId)
                         && p.PaymentStatus == PaymentStatus.Pending)
                .ToListAsync(ct);

            foreach (var payment in pendingPayments)
                payment.MarkFailed("Event cancelled", DateTime.UtcNow);
        }

        // ── 3. Handle Paid orders ─────────────────────────────────────
        var paidOrders = await db.Orders
            .IgnoreQueryFilters()
            .Where(o => o.EventId == notification.EventId
                     && o.TenantId == notification.TenantId
                     && o.OrderStatus == OrderStatus.Paid)
            .ToListAsync(ct);

        if (paidOrders.Count > 0)
        {
            var paidOrderIds = paidOrders
                .Select(o => o.Id)
                .ToList();

            var validTickets = await db.Tickets
                .IgnoreQueryFilters()
                .Where(t => paidOrderIds.Contains(t.OrderId)
                         && t.Status == TicketStatus.Valid)
                .ToListAsync(ct);

            foreach (var ticket in validTickets)
                ticket.Cancel();
        }

        // ── 4. Persist all changes ────────────────────────────────────
        await db.SaveChangesAsync(ct);

        // ── 5. Notify all affected attendees via RabbitMQ ─────────────
        // Persist first — publish after. If publish fails MassTransit
        // retries independently. If save fails we never publish.
        var allOrders = pendingOrders.Concat(paidOrders).ToList();

        if (allOrders.Count == 0)
            return;

        var attendeeIds = allOrders
            .Select(o => o.AttendeeId)
            .Distinct()
            .ToList();

        var attendees = await db.Users
            .IgnoreQueryFilters()
            .Where(u => attendeeIds.Contains(u.Id))
            .Select(u => new { u.Id, u.Email, u.Name })
            .ToListAsync(ct);

        foreach (var order in pendingOrders)
        {
            var attendee = attendees.FirstOrDefault(a => a.Id == order.AttendeeId);
            if (attendee is null) continue;

            await publishEndpoint.Publish(new EventCancelledMessage(
                notification.EventName,
                attendee.Email,
                attendee.Name,
                order.OrderNumber,
                WasPaid: false), ct);
        }

        foreach (var order in paidOrders)
        {
            var attendee = attendees.FirstOrDefault(a => a.Id == order.AttendeeId);
            if (attendee is null) continue;

            await publishEndpoint.Publish(new EventCancelledMessage(
                notification.EventName,
                attendee.Email,
                attendee.Name,
                order.OrderNumber,
                WasPaid: true), ct);
        }
    }
}
