using EventFlow.Application.Common.Contracts;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Tickets.Dtos;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Tickets.Enums;

namespace EventFlow.Application.Features.Tickets.Queries.GetMyTickets;

// Attendee views their own tickets (paginated, cached)
public sealed record GetMyTicketsQuery : PageRequest,
    ICachedQuery<Result<PaginatedList<TicketSummaryDto>>>
{
    public required Guid AttendeeId { get; init; }
    public TicketStatus? Status { get; init; }

    public string CacheKey =>
        $"attendee-tickets:{AttendeeId}:{Status}:{Page}:{PageSize}";

    public string[] Tags => [$"attendee-{AttendeeId}-tickets"];

    public TimeSpan Expiration => TimeSpan.FromMinutes(5);
}
