using EventFlow.Application.Common.Contracts;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Orders.Dtos;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Orders.Enums;

namespace EventFlow.Application.Features.Orders.Queries.GetEventsOrders;

public sealed record GetEventOrdersQuery : PageRequest,
    ICachedQuery<Result<PaginatedList<OrderSummaryDto>>>
{
    public required Guid EventId { get; init; }
    public Guid? TenantId { get; init; }
    public OrderStatus? Status { get; init; }

    public string CacheKey =>
        $"tenant-orders:{TenantId}:event:{EventId}:{Status}:{Page}:{PageSize}";

    public string[] Tags =>
    [
        $"tenant-{TenantId}-orders",
        $"event-{EventId}-orders"
    ];

    public TimeSpan Expiration => TimeSpan.FromMinutes(5);
}
