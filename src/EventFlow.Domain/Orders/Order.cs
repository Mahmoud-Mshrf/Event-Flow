using EventFlow.Domain.Common;
using EventFlow.Domain.Events;
using EventFlow.Domain.Orders.Enums;
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

    private readonly List<Payment> _payments = [];
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

    private readonly List<Ticket> _tickets = [];
    public IReadOnlyCollection<Ticket> Tickets => _tickets.AsReadOnly();

    private Order() { } // EF Core

}
