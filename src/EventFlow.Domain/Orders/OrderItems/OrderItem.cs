using EventFlow.Domain.Common;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.TicketTypes;

namespace EventFlow.Domain.Orders.OrderItems;

public class OrderItem : AuditableEntity
{
    public Guid TenantId { get; private set; }

    public Guid OrderId { get; private set; }
    public Guid TicketTypeId { get; private set; }

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    // Navigation properties
    public Order Order { get; private set; } = null!;

    public TicketType TicketType { get; private set; } = null!;

    private OrderItem() { }

    private OrderItem(
        Guid id,
        Guid tenantId,
        Guid orderId,
        Guid ticketTypeId,
        int quantity,
        decimal unitPrice) : base(id)
    {
        TenantId = tenantId;
        OrderId = orderId;
        TicketTypeId = ticketTypeId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public static Result<OrderItem> Create(
        Guid id,
        Guid tenantId,
        Guid orderId,
        Guid ticketTypeId,
        int quantity,
        decimal unitPrice)
    {
        if (tenantId == Guid.Empty)
            return OrderItemErrors.TenantIdRequired;

        if (orderId == Guid.Empty)
            return OrderItemErrors.OrderIdRequired;

        if (ticketTypeId == Guid.Empty)
            return OrderItemErrors.TicketTypeIdRequired;

        if (quantity <= 0)
            return OrderItemErrors.InvalidQuantity;

        if (unitPrice < 0)
            return OrderItemErrors.InvalidUnitPrice;

        var orderItem = new OrderItem(
            id,
            tenantId,
            orderId,
            ticketTypeId,
            quantity,
            unitPrice);

        return orderItem;
    }
}