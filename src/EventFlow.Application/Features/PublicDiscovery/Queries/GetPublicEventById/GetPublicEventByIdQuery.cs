using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.PublicDiscovery.Dtos;
using EventFlow.Domain.Common.Results;

namespace EventFlow.Application.Features.PublicDiscovery.Queries.GetPublicEventById;

public sealed record GetPublicEventByIdQuery(
    Guid EventId) : ICachedQuery<Result<PublicEventDetailDto>>
{
    public string CacheKey => $"public-event:{EventId}";

    public string[] Tags => ["public-events", $"public-event-{EventId}"];

    public TimeSpan Expiration => TimeSpan.FromMinutes(5);
}
