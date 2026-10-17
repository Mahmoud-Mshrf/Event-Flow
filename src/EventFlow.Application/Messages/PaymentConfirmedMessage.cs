namespace EventFlow.Application.Messages;

public sealed record PaymentConfirmedMessage(
    Guid PaymentId,
    Guid OrderId,
    Guid EventId,
    Guid TenantId,
    Guid AttendeeId,
    string AttendeeEmail,
    string AttendeeName,
    string EventName,
    string OrderNumber,
    IReadOnlyCollection<OrderItemMessage> Items);

// Carries what the consumers need — avoids them loading from DB twice
public sealed record OrderItemMessage(
    Guid TicketTypeId,
    string TicketTypeName,
    int Quantity);
