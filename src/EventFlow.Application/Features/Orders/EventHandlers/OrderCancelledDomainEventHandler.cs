using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Orders.Events;
using EventFlow.Domain.Payments.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Orders.EventHandlers;

internal sealed class OrderCancelledDomainEventHandler(
    IAppDbContext db,
    IDateTimeProvider dateTimeProvider)
    : INotificationHandler<OrderCancelledDomainEvent>
{
    public async Task Handle(OrderCancelledDomainEvent notification, CancellationToken ct)
    {
        var orderItems = await db.OrderItems
            .IgnoreQueryFilters()
            .Where(oi => oi.OrderId == notification.OrderId)
            .ToListAsync(ct);

        var ticketTypeIds = orderItems.Select(oi => oi.TicketTypeId).Distinct().ToList();

        var ticketTypes = await db.TicketTypes
            .IgnoreQueryFilters()
            .Where(tt => ticketTypeIds.Contains(tt.Id))
            .ToListAsync(ct);

        foreach (var item in orderItems)
            ticketTypes.FirstOrDefault(tt => tt.Id == item.TicketTypeId)?.Release(item.Quantity);

        var pendingPayments = await db.Payments
            .IgnoreQueryFilters()
            .Where(p => p.OrderId == notification.OrderId
                     && p.PaymentStatus == PaymentStatus.Pending)
            .ToListAsync(ct);

        foreach (var payment in pendingPayments)
            payment.MarkFailed("Order cancelled", dateTimeProvider.UtcNow);

        await db.SaveChangesAsync(ct);
    }
}