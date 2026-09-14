using System.ComponentModel.DataAnnotations.Schema;
using EventFlow.Domain.Common;
using EventFlow.Domain.Common.Results;
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
    [NotMapped]
    public bool IsValid => Status == TicketStatus.Valid;
    // Navigation properties
    public Event Event { get; private set; } = null!;
    public User Attendee { get; private set; } = null!;
    public TicketType TicketType { get; private set; } = null!;
    public Order Order { get; private set; } = null!;

    private Ticket() {}
    private Ticket(
    Guid id,
    Guid eventId,
    Guid tenantId,
    Guid attendeeId,
    Guid ticketTypeId,
    Guid orderId,
    string ticketNumber,
    string qrCode)
    : base(id)
    {
        EventId = eventId;
        TenantId = tenantId;
        AttendeeId = attendeeId;
        TicketTypeId = ticketTypeId;
        OrderId = orderId;
        TicketNumber = ticketNumber;
        QrCode = qrCode;
        Status = TicketStatus.Valid;
    }
    public static Result<Ticket> Create(
    Guid id,
    Guid eventId,
    Guid tenantId,
    Guid attendeeId,
    Guid ticketTypeId,
    Guid orderId,
    string ticketNumber,
    string qrCode)
    {
        if (eventId == Guid.Empty)
            return TicketErrors.EventIdRequired;

        if (tenantId == Guid.Empty)
            return TicketErrors.TenantIdRequired;

        if (attendeeId == Guid.Empty)
            return TicketErrors.AttendeeIdRequired;

        if (ticketTypeId == Guid.Empty)
            return TicketErrors.TicketTypeIdRequired;

        if (orderId == Guid.Empty)
            return TicketErrors.OrderIdRequired;

        if (string.IsNullOrWhiteSpace(ticketNumber))
            return TicketErrors.InvalidTicketNumber;

        if (string.IsNullOrWhiteSpace(qrCode))
            return TicketErrors.InvalidQrCode;

        var ticket = new Ticket(
            id,
            eventId,
            tenantId,
            attendeeId,
            ticketTypeId,
            orderId,
            ticketNumber,
            qrCode);

        return ticket;
    }

    public Result<Success> CheckIn(DateTime checkedInAt)
    {
        if (Status == TicketStatus.CheckedIn)
            return TicketErrors.AlreadyCheckedIn;

        if (Status == TicketStatus.Cancelled)
            return TicketErrors.TicketCancelled;

        if (Status != TicketStatus.Valid)
            return TicketErrors.CannotCheckIn;

        Status = TicketStatus.CheckedIn;
        CheckedInAt = checkedInAt;

        return Result.Success;
    }

    public Result<Success> Cancel()
    {
        if (Status == TicketStatus.Cancelled)
            return TicketErrors.CannotCancel;

        if (Status == TicketStatus.CheckedIn)
            return TicketErrors.CannotCancel;

        Status = TicketStatus.Cancelled;

        return Result.Success;
    }

}

