using EventFlow.Application.Common.Helpers;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Payments;
using EventFlow.Domain.Payments.Events;
using EventFlow.Domain.Tickets;
using EventFlow.Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace EventFlow.Application.Features.Payments.EventsHandlers;

// EventFlow.Application/Features/Tickets/DomainEventHandlers/PaymentSucceededDomainEventHandler.cs
public sealed class PaymentSucceededDomainEventHandler(
    IAppDbContext db,
    IQrCodeService qrCodeService,
    IEmailSender emailSender,HybridCache cache)
    : INotificationHandler<PaymentSucceededDomainEvent>
{
    public async Task Handle(
        PaymentSucceededDomainEvent notification,
        CancellationToken ct)
    {
        // 1. Load the order with all items and the attendee
        //    IgnoreQueryFilters — no tenant context in a domain event handler
        var order = await db.Orders
            .IgnoreQueryFilters()
            .Include(o => o.OrderItems)
            .Include(o => o.Attendee)
            .FirstOrDefaultAsync(o => o.Id == notification.OrderId, ct);

        if (order is null)
            return; // should never happen — log in production

        // 2. Load ticket type names for each order item
        //    Needed for ticket display and for the confirmation email
        var ticketTypeIds = order.OrderItems
            .Select(i => i.TicketTypeId)
            .Distinct()
            .ToList();

        var ticketTypes = await db.TicketTypes
            .IgnoreQueryFilters()
            .Where(tt => ticketTypeIds.Contains(tt.Id))
            .Select(tt => new { tt.Id, tt.Name })
            .ToDictionaryAsync(tt => tt.Id, tt => tt.Name, ct);

        // 3. Load event name for the email
        var @event = await db.Events
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == order.EventId, ct);

        if (@event is null)
            return;

        // 4. Issue one ticket per unit of quantity across all order items
        //    If an order has: 2x General + 1x VIP
        //    This creates: Ticket1 (General), Ticket2 (General), Ticket3 (VIP)
        var issuedTickets = new List<Ticket>();

        foreach (var item in order.OrderItems)
        {
            var ticketTypeName = ticketTypes.GetValueOrDefault(
                item.TicketTypeId, "Unknown");

            for (var i = 0; i < item.Quantity; i++)
            {
                var ticketId = Guid.NewGuid();

                // Generate signed QR payload — unforgeable without the secret
                var qrPayload = qrCodeService.GeneratePayload(ticketId);

                // Generate human-readable ticket number
                var ticketNumber = TicketNumberGenerator.Generate();

                var ticketResult = Ticket.Create(
                    ticketId,
                    order.EventId,
                    order.TenantId,
                    order.AttendeeId,
                    item.TicketTypeId,
                    order.Id,
                    ticketNumber,
                    qrPayload);

                if (ticketResult.IsError)
                    continue; // log in production — should never fail here

                issuedTickets.Add(ticketResult.Value);
            }
        }

        // 5. Persist all tickets in one batch
        await db.Tickets.AddRangeAsync(issuedTickets, ct);
        await db.SaveChangesAsync(ct);

        // Inject HybridCache and invalidate after tickets are created
        await cache.RemoveByTagAsync($"attendee-{order.AttendeeId}-tickets", ct);
        await cache.RemoveByTagAsync($"event-{order.EventId}-tickets", ct);

        // 6. Send confirmation email — after tickets are persisted
        //    If email fails, tickets still exist and attendee can view them in the app
        //    Use try/catch so email failure doesn't bubble up and cause retries
        try
        {
            await emailSender.SendTicketConfirmationAsync(
                order.Attendee.Email,
                order.Attendee.Name,
                @event.EventName,
                order.OrderNumber,
                issuedTickets.Select(t => new IssuedTicketInfo(
                    t.TicketNumber,
                    ticketTypes.GetValueOrDefault(t.TicketTypeId, "Unknown"),
                    t.QrCode)).ToList(),
                ct);
        }
        catch
        {
            // Log but don't rethrow — tickets are already issued
            // Attendee can view tickets in app regardless of email status
        }
    }
}