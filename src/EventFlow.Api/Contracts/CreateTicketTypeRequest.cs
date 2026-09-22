namespace EventFlow.Api.Contracts;
public sealed record CreateTicketTypeRequest(
    string Name,
    decimal Price,
    int Capacity,
    DateTime SalesStart,
    DateTime SalesEnd);

public sealed record UpdateTicketTypeNameRequest(string Name);
public sealed record UpdateTicketTypePriceRequest(decimal Price);
public sealed record UpdateTicketTypeCapacityRequest(int Capacity);
public sealed record UpdateTicketTypeSalesWindowRequest(DateTime SalesStart, DateTime SalesEnd);