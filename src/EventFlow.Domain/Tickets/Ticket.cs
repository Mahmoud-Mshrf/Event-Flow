using EventFlow.Domain.Common;
using EventFlow.Domain.Events;
using EventFlow.Domain.Orders;
using EventFlow.Domain.Tickets.Enums;
using EventFlow.Domain.TicketTypes;
using EventFlow.Domain.Users;

namespace EventFlow.Domain.Tickets;

public class Ticket:AuditableEntity
{
    public Guid EventId { get; private set; }
    public Guid TenantId { get; private set; }

    public Guid AttendeeId { get; private set; }
    public Guid TicketTypeId { get; private set; }
    public Guid OrderId { get; private set; }

    public string TicketNumber { get; private set; } = null!;

    public TicketStatus Status { get; private set; }

    public string QrCode { get; private set; } = null!;

    public DateTime? CheckedInAt { get; private set; }

    // Navigation properties
    public Event Event { get; private set; } = null!;

    public User Attendee { get; private set; } = null!;

    public TicketType TicketType { get; private set; } = null!;

    public Order Order { get; private set; } = null!;
}

