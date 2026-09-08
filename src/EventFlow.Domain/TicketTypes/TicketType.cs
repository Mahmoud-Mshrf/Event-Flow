using EventFlow.Domain.Events;
using EventFlow.Domain.Orders.OrderItems;
using EventFlow.Domain.Tickets;

namespace EventFlow.Domain.TicketTypes;

public class TicketType
{
    public Guid Id { get; private set; }

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

    public ICollection<Ticket> Tickets { get; private set; }
        = new List<Ticket>();

    public ICollection<OrderItem> OrderItems { get; private set; }
        = new List<OrderItem>();
}