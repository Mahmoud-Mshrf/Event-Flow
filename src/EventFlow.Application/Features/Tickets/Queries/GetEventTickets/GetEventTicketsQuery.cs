using EventFlow.Application.Common.Contracts;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Tickets.Dtos;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Tickets.Enums;

namespace EventFlow.Application.Features.Tickets.Queries.GetEventTickets;

// Organizer views all tickets for an event
public sealed record GetEventTicketsQuery : PageRequest,
    ICachedQuery<Result<PaginatedList<TicketSummaryDto>>>
{
    public required Guid EventId { get; init; }
    public Guid? TenantId { get; init; }
    public TicketStatus? Status { get; init; }

    public string CacheKey =>
        $"tenant-tickets:{TenantId}:event:{EventId}:{Status}:{Page}:{PageSize}";

    public string[] Tags =>
    [
        $"tenant-{TenantId}-tickets",
        $"event-{EventId}-tickets"
    ];

    public TimeSpan Expiration => TimeSpan.FromMinutes(5);
}
