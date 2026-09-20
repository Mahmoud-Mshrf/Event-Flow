using EventFlow.Domain.Common;

namespace EventFlow.Domain.Users.Events;

public sealed record PasswordResetRequestedDomainEvent(
    string Email,
    string RawCode) : DomainEvent;