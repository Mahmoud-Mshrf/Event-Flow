using EventFlow.Domain.Common;
namespace EventFlow.Domain.Events.DomainEvents;
public sealed record EventCancelledDomainEvent(
    Guid EventId,
    Guid TenantId) : DomainEvent;