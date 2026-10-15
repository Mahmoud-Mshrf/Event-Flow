using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Orders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Orders.EventHandlers;

internal sealed class OrderExpiredDomainEventHandler(IAppDbContext dbContext)
    : INotificationHandler<OrderExpiredDomainEvent>
{
    public async Task Handle(OrderExpiredDomainEvent notification, CancellationToken ct)
    {
        var orderItems = await dbContext.OrderItems
            .IgnoreQueryFilters()
            .Where(oi => oi.OrderId == notification.OrderId)
            .ToListAsync(ct);

        if (orderItems.Count == 0)
            return;

        var ticketTypeIds = orderItems
            .Select(oi => oi.TicketTypeId)
            .Distinct()
            .ToList();

        var ticketTypes = await dbContext.TicketTypes
            .IgnoreQueryFilters()
            .Where(tt => ticketTypeIds.Contains(tt.Id))
            .ToListAsync(ct);

        foreach (var item in orderItems)
        {
            var ticketType = ticketTypes.FirstOrDefault(tt => tt.Id == item.TicketTypeId);
            ticketType?.Release(item.Quantity);
        }

        await dbContext.SaveChangesAsync(ct);
    }
}