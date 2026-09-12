using EventFlow.Domain.Common;
using EventFlow.Domain.Common.Results;
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

    // Constructor for EF Core
    private TicketType() { }
    // Constructor for creating a new TicketType
    private TicketType(Guid id, Guid eventId,
    Guid tenantId,
    string name,
    decimal price,
    int capacity,
    DateTime salesStart,
    DateTime salesEnd):base(id)
    {
        EventId = eventId;
        TenantId = tenantId;
        Name = name;
        Price = price;
        Capacity = capacity;
        ReservedQuantity = 0;
        SalesStart = salesStart;
        SalesEnd = salesEnd;
    }
    public static Result<TicketType> Create(Guid id,
    Guid eventId,
    Guid tenantId,
    string name,
    decimal price,
    int capacity,
    DateTime salesStart,
    DateTime salesEnd)
    {
        if (eventId == Guid.Empty)
            return TicketTypeErrors.EventIdRequired;

        if (tenantId == Guid.Empty)
            return TicketTypeErrors.TenantIdRequired;

        if (string.IsNullOrWhiteSpace(name))
            return TicketTypeErrors.InvalidName;

        if (price < 0)
            return TicketTypeErrors.InvalidPrice;

        if (capacity <= 0)
            return TicketTypeErrors.InvalidCapacity;

        if (salesStart >= salesEnd)
            return TicketTypeErrors.InvalidSalesWindow;

        var ticketType = new TicketType(id, eventId, tenantId, name, price, capacity, salesStart, salesEnd);
        return ticketType;
    }
    public Result<Updated> UpdateDetails(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 30 || 3 > name.Length)
            return TicketTypeErrors.InvalidName;
        Name = name.Trim();
        return Result.Updated;
    }
    public Result<Updated> UpdatePrice(decimal price)
    {
        if (price < 0)
            return TicketTypeErrors.InvalidPrice;

        Price = price;

        return Result.Updated;
    }
}
