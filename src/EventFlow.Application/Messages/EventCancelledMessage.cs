namespace EventFlow.Application.Messages;

public sealed record EventCancelledMessage(
    string EventName,
    string AttendeeEmail,
    string AttendeeName,
    string OrderNumber,
    bool WasPaid);
