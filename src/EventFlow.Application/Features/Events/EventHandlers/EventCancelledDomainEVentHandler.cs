using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Events.DomainEvents;
using EventFlow.Domain.Orders.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Events.EventHandlers;

public sealed class EventCancelledDomainEventHandler(
    IAppDbContext db)
    : INotificationHandler<EventCancelledDomainEvent>
{
    public async Task Handle(
        EventCancelledDomainEvent notification,
        CancellationToken ct)
    {
        // 1. Find all Pending orders for this event
        var pendingOrders = await db.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.EventId == notification.EventId
                && o.TenantId == notification.TenantId
                && o.OrderStatus == OrderStatus.Pending)
            .ToListAsync(ct);

        if (pendingOrders.Count == 0)
            return;

        // 2. Collect all TicketTypeIds that need capacity released
        var ticketTypeIds = pendingOrders
            .SelectMany(o => o.OrderItems)
            .Select(i => i.TicketTypeId)
            .Distinct()
            .ToList();

        var ticketTypes = await db.TicketTypes
            .Where(tt => ticketTypeIds.Contains(tt.Id))
            .ToListAsync(ct);

        // 3. Cancel each order and release capacity
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

        await db.SaveChangesAsync(ct);

        // Note: attendee notification (email) would be a second domain event
        // or a RabbitMQ message dispatched here — deferred to messaging module
    }
}