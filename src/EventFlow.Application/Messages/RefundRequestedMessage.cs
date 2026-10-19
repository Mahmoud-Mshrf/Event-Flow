namespace EventFlow.Application.Messages;

public sealed record RefundRequestedMessage(
    Guid PaymentId,
    string ProviderTransactionId,  // Paymob's txn ID — passed directly to refund API
    decimal Amount,
    string OrderNumber,
    string AttendeeEmail,
    string AttendeeName,
    string EventName);