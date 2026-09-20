using EventFlow.Domain.Common;

namespace EventFlow.Domain.Users.Events;

public sealed record PasswordResetCompletedDomainEvent(
    string Email) : DomainEvent;
