using EventFlow.Domain.Common;

namespace EventFlow.Domain.Users.Events;  

public sealed record AttendeeRegisteredDomainEvent(
    Guid UserId,
    string Email,
    string RawCode) : DomainEvent;