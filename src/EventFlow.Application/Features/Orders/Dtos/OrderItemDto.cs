namespace EventFlow.Application.Features.Orders.Dtos;

public sealed record OrderItemDto(
    Guid Id,
    Guid TicketTypeId,
    string TicketTypeName,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal);
