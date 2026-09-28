using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Common.Results;

namespace EventFlow.Application.Features.CheckIn.Queries.GetCheckInStats;

public sealed record GetCheckInStatsQuery(
    Guid EventId,
    Guid? TenantId) : ICachedQuery<Result<CheckInStatsDto>>
{
    public string CacheKey => $"checkin-stats:{TenantId}:{EventId}";
    public string[] Tags => [$"event-{EventId}-checkin"];

    // Short expiry — stats change frequently on event day
    public TimeSpan Expiration => TimeSpan.FromSeconds(30);
}

public sealed record CheckInStatsDto(
    Guid EventId,
    int TotalTickets,
    int CheckedIn,
    int Remaining);
