using EventFlow.Domain.Common;
using EventFlow.Domain.Events.Enums;
namespace EventFlow.Domain.Events.DomainEvents;
public sealed record EventPublishedDomainEvent(
    Guid EventId,
    Guid TenantId,
    EventVisibility Visibility) : DomainEvent;
