using MediatR;

namespace EventFlow.Domain.Common;
public abstract record DomainEvent : INotification;
