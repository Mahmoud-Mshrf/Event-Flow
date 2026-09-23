using EventFlow.Application.Features.Orders.Dtos;
using EventFlow.Domain.Orders;
using EventFlow.Domain.Orders.OrderItems;

namespace EventFlow.Application.Features.Orders.Mappers;

public static class OrderMapper
{
    public static OrderSummaryDto ToSummaryDto(this Order order, string eventName) => new(
        order.Id,
        order.OrderNumber,
        eventName,
        order.OrderStatus,
        order.Total,
        order.CreatedAtUtc.DateTime);

    public static OrderDto ToDto(
        this Order order,
        string eventName,
        IReadOnlyCollection<(OrderItem Item, string TicketTypeName)> items) => new(
        order.Id,
        order.OrderNumber,
        order.EventId,
        eventName,
        order.OrderStatus,
        null, // Payment status comes from Payment aggregate separately
        order.Total,
        order.CreatedAtUtc.DateTime,
        order.PaidAt,
        items.Select(x => new OrderItemDto(
            x.Item.Id,
            x.Item.TicketTypeId,
            x.TicketTypeName,
            x.Item.Quantity,
            x.Item.UnitPrice,
            x.Item.Subtotal)).ToList());
}