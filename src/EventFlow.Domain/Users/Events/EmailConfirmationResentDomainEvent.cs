using EventFlow.Domain.Common;

namespace EventFlow.Domain.Users.Events;

public sealed record EmailConfirmationResentDomainEvent(
    string Email,
    string RawCode) : DomainEvent;