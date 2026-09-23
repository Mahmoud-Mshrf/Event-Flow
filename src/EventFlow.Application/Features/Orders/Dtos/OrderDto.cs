using EventFlow.Domain.Orders.Enums;
using EventFlow.Domain.Payments.Enums;

namespace EventFlow.Application.Features.Orders.Dtos;

public sealed record OrderDto(
    Guid Id,
    string OrderNumber,
    Guid EventId,
    string EventName,
    OrderStatus OrderStatus,
    PaymentStatus? PaymentStatus,
    decimal Total,
    DateTime CreatedAt,
    DateTime? PaidAt,
    IReadOnlyCollection<OrderItemDto> Items);
