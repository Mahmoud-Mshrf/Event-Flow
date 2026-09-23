using EventFlow.Application.Common.Contracts;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.PublicDiscovery.Dtos;
using EventFlow.Domain.Common.Results;

namespace EventFlow.Application.Features.PublicDiscovery.Queries.GetPublicEvents;

public sealed record GetPublicEventsQuery : PageRequest,
    ICachedQuery<Result<PaginatedList<PublicEventDto>>>
{
    public string? SearchTerm { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }

    public string CacheKey =>
        $"public-events:{SearchTerm}:{FromDate:yyyyMMdd}:{ToDate:yyyyMMdd}:{Page}:{PageSize}";

    public string[] Tags => ["public-events"];

    public TimeSpan Expiration => TimeSpan.FromMinutes(5); // shorter — public data changes more visibly
}
