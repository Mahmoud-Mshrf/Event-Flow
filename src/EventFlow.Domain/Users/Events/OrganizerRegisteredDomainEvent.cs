using EventFlow.Domain.Common;

namespace EventFlow.Domain.Users.Events;

public sealed record OrganizerRegisteredDomainEvent(
    string Email,
    string RawCode) : DomainEvent;