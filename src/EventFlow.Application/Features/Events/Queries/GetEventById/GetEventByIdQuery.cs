using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Events.Dtos;
using EventFlow.Domain.Common.Results;

namespace EventFlow.Application.Features.Events.Queries.GetEventById;

public sealed record GetEventByIdQuery(
    Guid EventId,
    Guid? TenantId) : ICachedQuery<Result<EventDto>>
{
    public string CacheKey => $"tenant-events:{TenantId}:{EventId}";

    public string[] Tags => [$"tenant-{TenantId}-events"];

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}
