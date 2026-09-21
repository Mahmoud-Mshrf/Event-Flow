using EventFlow.Application.Common.Contracts;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Events.Dtos;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Events.Enums;

namespace EventFlow.Application.Features.Events.Queries.GetOrganizerEvents;

public sealed record GetOrganizerEventsQuery : PageRequest, ICachedQuery<Result<PaginatedList<EventDto>>>
{
    public required Guid TenantId { get; init; }
    public EventStatus? Status { get; init; }
    public EventVisibility? Visibility { get; init; }

    public string CacheKey =>
        $"tenant-events:{TenantId}:all:{Status}:{Visibility}:{Page}:{PageSize}";

    public string[] Tags => [$"tenant-{TenantId}-events"];

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}
