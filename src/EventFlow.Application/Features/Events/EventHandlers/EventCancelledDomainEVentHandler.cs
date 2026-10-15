using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Events.DomainEvents;
using EventFlow.Domain.Orders.Enums;
using EventFlow.Domain.Payments.Enums;
using EventFlow.Domain.Tickets.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Events.EventHandlers;

internal sealed class EventCancelledDomainEventHandler(IAppDbContext db)
    : INotificationHandler<EventCancelledDomainEvent>
{
    public async Task Handle(
        EventCancelledDomainEvent notification,
        CancellationToken ct)
    {
        // ── 1. Handle Pending orders ─────────────────────────────────
        // Cancel them and release capacity back to TicketTypes
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

            // ── 2. Mark stale Pending payments as Failed ─────────────
            // Pending orders may have an initiated payment session that
            // will never complete now — mark it Failed so there are no
            // ghost Pending payments in the DB
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

        // ── 3. Handle Paid orders ────────────────────────────────────
        // These attendees already paid — we can't cancel their orders
        // (refund logic is out of scope for now) but we must cancel
        // their Valid tickets so they don't show up as valid at the door
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

        // ── 4. Persist all changes in one transaction ─────────────────
        await db.SaveChangesAsync(ct);

    }
}