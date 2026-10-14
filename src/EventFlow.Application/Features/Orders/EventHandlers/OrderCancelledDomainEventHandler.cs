using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Orders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Orders.EventHandlers;

public sealed class OrderCancelledDomainEventHandler(IAppDbContext db)
    : INotificationHandler<OrderCancelledDomainEvent>
{
    public async Task Handle(OrderCancelledDomainEvent notification, CancellationToken cancellationToken)
    {
        if (notification.Items.Count == 0)
            return;

        var releasesByTicketType = notification.Items
            .GroupBy(i => i.TicketTypeId)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

        var ticketTypes = await db.TicketTypes
            .Where(tt => releasesByTicketType.Keys.Contains(tt.Id)
                && tt.TenantId == notification.TenantId)
            .ToListAsync(cancellationToken);

        foreach (var (ticketTypeId, quantity) in releasesByTicketType)
        {
            var ticketType = ticketTypes.FirstOrDefault(tt => tt.Id == ticketTypeId);
            if (ticketType is null)
                continue;

            ticketType.Release(quantity);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}

public sealed class OrderExpiredDomainEventHandler(IAppDbContext db)
    : INotificationHandler<OrderExpiredDomainEvent>
{
    public async Task Handle(OrderExpiredDomainEvent notification, CancellationToken cancellationToken)
    {
        if (notification.Items.Count == 0)
            return;

        var releasesByTicketType = notification.Items
            .GroupBy(i => i.TicketTypeId)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

        var ticketTypes = await db.TicketTypes
            .Where(tt => releasesByTicketType.Keys.Contains(tt.Id)
                && tt.TenantId == notification.TenantId)
            .ToListAsync(cancellationToken);

        foreach (var (ticketTypeId, quantity) in releasesByTicketType)
        {
            var ticketType = ticketTypes.FirstOrDefault(tt => tt.Id == ticketTypeId);
            if (ticketType is null)
                continue;

            ticketType.Release(quantity);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}