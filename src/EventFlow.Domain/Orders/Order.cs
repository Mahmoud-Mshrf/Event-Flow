using System.Data.Common;
using EventFlow.Domain.Common;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Events;
using EventFlow.Domain.Orders.Enums;
using EventFlow.Domain.Orders.Events;
using EventFlow.Domain.Orders.OrderItems;
using EventFlow.Domain.Payments;
using EventFlow.Domain.Tickets;
using EventFlow.Domain.Users;

namespace EventFlow.Domain.Orders;

public class Order : AuditableEntity
{
    public Guid TenantId { get; private set; }
    public Guid EventId { get; private set; }
    public Guid AttendeeId { get; private set; }
    public string OrderNumber { get; private set; } = null!;
    public OrderStatus OrderStatus { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public decimal Total { get; private set; }

    public Event Event { get; private set; } = null!;
    public User Attendee { get; private set; } = null!;

    private readonly List<OrderItem> _orderItems = [];
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    private Order() { } // EF Core

    private Order(Guid id, Guid tenantId, Guid eventId, Guid attendeeId, string orderNumber, List<OrderItem> items)
        : base(id)
    {
        TenantId = tenantId;
        EventId = eventId;
        AttendeeId = attendeeId;
        OrderNumber = orderNumber;
        OrderStatus = OrderStatus.Pending;
        _orderItems.AddRange(items);
        Total = items.Sum(i => i.Subtotal);
    }

    public static Result<Order> Create(
        Guid id,
        Guid tenantId,
        Guid eventId,
        Guid attendeeId,
        string orderNumber,
        IReadOnlyCollection<(Guid Id,Guid tenantId, Guid TicketTypeId, int Quantity, decimal UnitPrice)> requestedItems)
    {
        if (requestedItems is null || requestedItems.Count == 0)
            return OrderErrors.NoItems;

        if (requestedItems.Any(i => i.Quantity <= 0))
            return OrderErrors.InvalidItemQuantity;

        var items = requestedItems
            .Select(i => OrderItem.Create(i.Id,i.tenantId ,i.TicketTypeId, i.Quantity, i.UnitPrice))
            .ToList();

        return new Order(id, tenantId, eventId, attendeeId, orderNumber, items);
    }

    public Result<Success> MarkAsPaid(DateTime paidAtUtc)
    {
        if (OrderStatus != OrderStatus.Pending)
            return OrderErrors.CannotPayNonPendingOrder;

        OrderStatus = OrderStatus.Paid;
        PaidAt = paidAtUtc;

        return Result.Success;
    }

    public Result<Success> MarkAsExpired()
    {
        if (OrderStatus != OrderStatus.Pending)
            return OrderErrors.CannotExpireNonPendingOrder;

        OrderStatus = OrderStatus.Expired;

        AddDomainEvent(new OrderExpiredDomainEvent(Id, TenantId, ReservedItems()));

        return Result.Success;
    }

    public Result<Success> Cancel()
    {
        if (OrderStatus != OrderStatus.Pending)
            return OrderErrors.CannotCancelNonPendingOrder;

        OrderStatus = OrderStatus.Cancelled;

        AddDomainEvent(new OrderCancelledDomainEvent(Id, TenantId, ReservedItems()));

        return Result.Success;
    }

    private IReadOnlyCollection<(Guid TicketTypeId, int Quantity)> ReservedItems() =>
        _orderItems.Select(i => (i.TicketTypeId, i.Quantity)).ToList();
}

