using EventFlow.Domain.Orders.Enums;

namespace EventFlow.Application.Features.Orders.Dtos;

public sealed record OrderSummaryDto(
    Guid Id,
    string OrderNumber,
    string EventName,
    OrderStatus OrderStatus,
    decimal Total,
    DateTime CreatedAt);