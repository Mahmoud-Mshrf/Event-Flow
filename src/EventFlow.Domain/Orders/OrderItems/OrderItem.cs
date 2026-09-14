using EventFlow.Domain.Common;
using EventFlow.Domain.TicketTypes;

namespace EventFlow.Domain.Orders.OrderItems;

public class OrderItem:AuditableEntity
{
    public Guid TenantId { get; private set; }

    public Guid OrderId { get; private set; }
    public Guid TicketTypeId { get; private set; }

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    // Navigation properties
    public Order Order { get; private set; } = null!;

    public TicketType TicketType { get; private set; } = null!;
}
