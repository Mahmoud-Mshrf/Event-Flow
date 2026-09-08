using EventFlow.Domain.Events;
using EventFlow.Domain.Orders.Enums;
using EventFlow.Domain.Orders.OrderItems;
using EventFlow.Domain.Payments;
using EventFlow.Domain.Tickets;
using EventFlow.Domain.Users;

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

    // Navigation properties
    public Event Event { get; private set; } = null!;

    public User Attendee { get; private set; } = null!;

    public Payment Payment { get; private set; } = null!;

    public ICollection<OrderItem> OrderItems { get; private set; }
        = new List<OrderItem>();

    public ICollection<Ticket> Tickets { get; private set; }
        = new List<Ticket>();
}