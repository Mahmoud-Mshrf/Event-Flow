namespace EventFlow.Application.Features.TicketTypes.Dtos;

public sealed record TicketTypeDto(
    Guid Id,
    Guid EventId,
    string Name,
    decimal Price,
    int Capacity,
    int ReservedQuantity,
    int AvailableQuantity,
    DateTime SalesStart,
    DateTime SalesEnd,
    bool IsSalesOpen);