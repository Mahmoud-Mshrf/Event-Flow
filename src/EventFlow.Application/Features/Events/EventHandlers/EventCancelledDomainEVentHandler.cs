using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Messages;
using EventFlow.Domain.Events.DomainEvents;
using EventFlow.Domain.Orders.Enums;
using EventFlow.Domain.Payments.Enums;
using EventFlow.Domain.Tickets.Enums;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventFlow.Application.Features.Events.EventHandlers;

internal sealed class EventCancelledDomainEventHandler(
    IAppDbContext db,
    IPublishEndpoint publishEndpoint,
    ILogger<EventCancelledDomainEventHandler> logger)
    : INotificationHandler<EventCancelledDomainEvent>
{
    public async Task Handle(
        EventCancelledDomainEvent notification,
        CancellationToken ct)
    {
        // ── 1. Pending orders ─────────────────────────────────────────
        // Cancel() raises OrderCancelledDomainEvent. Its handler releases
        // the reserved capacity and fails the pending payment, so we do
        // NOT do that here (it would release capacity twice).
        var pendingOrders = await db.Orders
            .IgnoreQueryFilters()
            .Where(o => o.EventId == notification.EventId
                     && o.TenantId == notification.TenantId
                     && o.OrderStatus == OrderStatus.Pending)
            .ToListAsync(ct);

        foreach (var order in pendingOrders)
            order.Cancel();

        // ── 2. Paid orders ────────────────────────────────────────────
        // The order stays Paid (the money was really taken), but its valid
        // tickets must stop working. Checked-in tickets are left alone:
        // Ticket.Cancel() rejects them.
        var paidOrders = await db.Orders
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(o => o.EventId == notification.EventId
                     && o.TenantId == notification.TenantId
                     && o.OrderStatus == OrderStatus.Paid)
            .ToListAsync(ct);

        var paidOrderIds = paidOrders.Select(o => o.Id).ToList();

        var validTickets = await db.Tickets
            .IgnoreQueryFilters()
            .Where(t => paidOrderIds.Contains(t.OrderId)
                     && t.Status == TicketStatus.Valid)
            .ToListAsync(ct);

        foreach (var ticket in validTickets)
            ticket.Cancel();

        if (pendingOrders.Count == 0 && paidOrders.Count == 0)
            return;

        // ── 3. Load what the messages need, before saving ─────────────
        // Payments we can refund: succeeded, with Paymob's transaction id
        var succeededPayments = await db.Payments
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(p => paidOrderIds.Contains(p.OrderId)
                     && p.PaymentStatus == PaymentStatus.Succeeded)
            .ToListAsync(ct);

        var paymentByOrderId = succeededPayments
            .GroupBy(p => p.OrderId)
            .ToDictionary(g => g.Key, g => g.First());

        // One batch query for every affected attendee.
        // User has no global filter, and we select by id, so this is scoped.
        var attendeeIds = pendingOrders.Select(o => o.AttendeeId)
            .Concat(paidOrders.Select(o => o.AttendeeId))
            .Distinct()
            .ToList();

        var attendees = await db.Users
            .AsNoTracking()
            .Where(u => attendeeIds.Contains(u.Id))
            .Select(u => new { u.Id, u.Email, u.Name })
            .ToDictionaryAsync(u => u.Id, ct);

        // ── 4. Persist ────────────────────────────────────────────────
        // Dispatches OrderCancelledDomainEvent for each pending order
        await db.SaveChangesAsync(ct);

        // ── 5. Notify and refund ──────────────────────────────────────
        // One message per attendee and per refund: each retries on its own.
        foreach (var order in pendingOrders)
        {
            if (!attendees.TryGetValue(order.AttendeeId, out var attendee))
                continue;

            await publishEndpoint.Publish(new EventCancelledMessage(
                notification.EventName,
                attendee.Email,
                attendee.Name,
                order.OrderNumber,
                WasPaid: false), ct);
        }

        foreach (var order in paidOrders)
        {
            if (!attendees.TryGetValue(order.AttendeeId, out var attendee))
                continue;

            await publishEndpoint.Publish(new EventCancelledMessage(
                notification.EventName,
                attendee.Email,
                attendee.Name,
                order.OrderNumber,
                WasPaid: true), ct);

            if (!paymentByOrderId.TryGetValue(order.Id, out var payment)
                || payment.ProviderTransactionId is null)
            {
                // Needs manual follow-up: we cannot refund without Paymob's id
                logger.LogWarning(
                    "No refundable payment found for paid order {OrderNumber} " +
                    "of cancelled event {EventId}.",
                    order.OrderNumber, notification.EventId);
                continue;
            }

            await publishEndpoint.Publish(new RefundRequestedMessage(
                PaymentId: payment.Id,
                ProviderTransactionId: payment.ProviderTransactionId,
                Amount: payment.Amount,
                OrderNumber: order.OrderNumber,
                AttendeeEmail: attendee.Email,
                AttendeeName: attendee.Name,
                EventName: notification.EventName), ct);
        }
    }
}