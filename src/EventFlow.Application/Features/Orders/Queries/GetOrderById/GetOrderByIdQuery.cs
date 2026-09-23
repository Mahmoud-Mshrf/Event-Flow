using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Orders.Dtos;
using EventFlow.Domain.Common.Results;

namespace EventFlow.Application.Features.Orders.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(
    Guid OrderId,
    Guid AttendeeId) : ICachedQuery<Result<OrderDto>>
{
    public string CacheKey => $"attendee-orders:{AttendeeId}:{OrderId}";
    public string[] Tags => [$"attendee-{AttendeeId}-orders"];
    public TimeSpan Expiration => TimeSpan.FromMinutes(5);
}
