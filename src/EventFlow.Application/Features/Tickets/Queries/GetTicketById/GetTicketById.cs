using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Tickets.Dtos;
using EventFlow.Domain.Common.Results;

namespace EventFlow.Application.Features.Tickets.Queries.GetTicketById;

// Attendee views one ticket with full detail and QR code
public sealed record GetTicketByIdQuery(
    Guid TicketId,
    Guid AttendeeId) : ICachedQuery<Result<TicketDto>>
{
    public string CacheKey => $"attendee-tickets:{AttendeeId}:{TicketId}";
    public string[] Tags => [$"attendee-{AttendeeId}-tickets"];
    public TimeSpan Expiration => TimeSpan.FromMinutes(5);
}

