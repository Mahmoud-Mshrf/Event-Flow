using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.TicketTypes.Dtos;
using EventFlow.Domain.Common.Results;

namespace EventFlow.Application.Features.TicketTypes.Queries.GetTicketTypeById;

public sealed record GetTicketTypeByIdQuery(
    Guid TicketTypeId,
    Guid? TenantId) : ICachedQuery<Result<TicketTypeDto>>
{
    public string CacheKey => $"tenant-ticket-types:{TenantId}:{TicketTypeId}";

    public string[] Tags =>
    [
        $"tenant-{TenantId}-ticket-types"
    ];

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}
