using EventFlow.Domain.Common;

namespace EventFlow.Domain.Tickets.Events;

public sealed record AttendeeCheckedInDomainEvent(
    Guid TicketId,
    Guid EventId,
    Guid TenantId,
    DateTime CheckedInAt) : DomainEvent;