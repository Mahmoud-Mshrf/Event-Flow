using EventFlow.Domain.Common;

namespace EventFlow.Domain.Payments.Events;

public sealed record PaymentSucceededDomainEvent(
    Guid PaymentId,
    Guid OrderId,
    Guid TenantId) : DomainEvent;