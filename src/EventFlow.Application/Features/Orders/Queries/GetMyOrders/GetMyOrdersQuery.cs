using EventFlow.Application.Common.Contracts;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Orders.Dtos;
using EventFlow.Domain.Common.Results;

namespace EventFlow.Application.Features.Orders.Queries.GetMyOrders;

public sealed record GetMyOrdersQuery : PageRequest,
    ICachedQuery<Result<PaginatedList<OrderSummaryDto>>>
{
    public required Guid AttendeeId { get; init; }

    public string CacheKey => $"attendee-orders:{AttendeeId}:{Page}:{PageSize}";
    public string[] Tags => [$"attendee-{AttendeeId}-orders"];
    public TimeSpan Expiration => TimeSpan.FromMinutes(5);
}
