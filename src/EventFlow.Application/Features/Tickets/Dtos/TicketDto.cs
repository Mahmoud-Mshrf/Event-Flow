using EventFlow.Domain.Tickets.Enums;

namespace EventFlow.Application.Features.Tickets.Dtos;

public sealed record TicketDto(
    Guid Id,
    string TicketNumber,
    string EventName,
    string EventLocation,
    DateTime EventStartDate,
    string TicketTypeName,
    TicketStatus Status,
    string QrCode,
    DateTime? CheckedInAt);

public sealed record TicketSummaryDto(
    Guid Id,
    string TicketNumber,
    string EventName,
    TicketStatus Status);


// Projection record used inside query handlers
// Not exposed to callers — used only for the SELECT shape
internal sealed record TicketProjection(
    Guid Id,
    string TicketNumber,
    string EventName,
    string EventLocation,
    DateTime EventStartDate,
    string TicketTypeName,
    TicketStatus Status,
    string QrCode,
    DateTime? CheckedInAt)
{
    public TicketDto ToDto() => new(
        Id,
        TicketNumber,
        EventName,
        EventLocation,
        EventStartDate,
        TicketTypeName,
        Status,
        QrCode,
        CheckedInAt);
}