using EventFlow.Domain.Common;
using EventFlow.Domain.Events;
using EventFlow.Domain.Orders.OrderItems;
using EventFlow.Domain.Tickets;

namespace EventFlow.Domain.TicketTypes;

public class TicketType:AuditableEntity
{
    public Guid EventId { get; private set; }
    public Guid TenantId { get; private set; }

    public string Name { get; private set; } = null!;
    public decimal Price { get; private set; }

    public int Capacity { get; private set; }
    public int ReservedQuantity { get; private set; }

    public DateTime SalesStart { get; private set; }
    public DateTime SalesEnd { get; private set; }

    public byte[] RowVersion { get; private set; } = null!;

    // Navigation properties
    public Event Event { get; private set; } = null!;

    private readonly List<Ticket> _tickets = [];
    public IReadOnlyCollection<Ticket> Tickets => _tickets;
    private readonly List<OrderItem> _orderItems = [];
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems;
}