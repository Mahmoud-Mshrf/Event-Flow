using EventFlow.Domain.Common;

namespace EventFlow.Domain.Orders.Events;
public sealed record OrderExpiredDomainEvent(
    Guid OrderId,
    Guid TenantId,
    IReadOnlyCollection<(Guid TicketTypeId, int Quantity)> Items) : DomainEvent;
