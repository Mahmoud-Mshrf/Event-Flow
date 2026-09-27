using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Payments;
using EventFlow.Domain.Payments.Events;
using EventFlow.Domain.Users;
using MediatR;

namespace EventFlow.Application.Features.Payments.EventsHandlers;

public sealed class PaymentSucceededDomainEventHandler(IAppDbContext context,ICurrentUser currentUser) : INotificationHandler<PaymentSucceededDomainEvent>
{
    public Task Handle(PaymentSucceededDomainEvent notification, CancellationToken cancellationToken)
    {

        throw new NotImplementedException();
    }
}