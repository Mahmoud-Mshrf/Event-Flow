using EventFlow.Domain.Orders.Enums;
using EventFlow.Domain.Orders.OrderItems;

namespace EventFlow.Domain.Orders;

public class Order
{
    public Guid Id { get; private set; }

    public Guid EventId { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid AttendeeId { get; private set; }

    public string OrderNumber { get; private set; } = null!;

    public OrderStatus OrderStatus { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }

    public decimal Total { get; private set; }

    public ICollection<OrderItem> OrderItems { get; private set; }
        = new List<OrderItem>();
}