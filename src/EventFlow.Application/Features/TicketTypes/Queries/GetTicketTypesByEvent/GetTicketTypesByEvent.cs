using EventFlow.Application.Common.Contracts;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.TicketTypes.Dtos;
using EventFlow.Domain.Common.Results;

namespace EventFlow.Application.Features.TicketTypes.Queries.GetTicketTypesByEvent;

public sealed record GetTicketTypesByEventQuery(
    Guid EventId,
    Guid? TenantId) : PageRequest, ICachedQuery<Result<PaginatedList<TicketTypeDto>>>
{
    public string CacheKey =>
        $"tenant-ticket-types:{TenantId}:event:{EventId}:{Page}:{PageSize}";

    public string[] Tags =>
    [
        $"tenant-{TenantId}-ticket-types",
        $"event-{EventId}-ticket-types"
    ];

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}
