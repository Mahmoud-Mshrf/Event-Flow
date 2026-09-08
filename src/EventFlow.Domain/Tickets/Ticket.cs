using EventFlow.Domain.Tickets.Enums;

namespace EventFlow.Domain.Tickets;

public class Ticket
{
    public Guid Id { get; private set; }

    public Guid EventId { get; private set; }
    public Guid TenantId { get; private set; }

    public Guid AttendeeId { get; private set; }
    public Guid TicketTypeId { get; private set; }
    public Guid OrderId { get; private set; }

    public string TicketNumber { get; private set; } = null!;

    public TicketStatus Status { get; private set; }

    public string QrCode { get; private set; } = null!;

    public DateTime? CheckedInAt { get; private set; }
}

