namespace EventFlow.Api.Contracts;

public sealed record CreateOrderItemRequest(
    Guid TicketTypeId,
    int Quantity);
