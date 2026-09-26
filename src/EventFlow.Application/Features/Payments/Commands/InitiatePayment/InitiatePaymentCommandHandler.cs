using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Orders;
using EventFlow.Domain.Orders.Enums;
using EventFlow.Domain.Payments;
using EventFlow.Domain.Payments.Enums;
using EventFlow.Domain.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Payments.Commands.InitiatePayment;

public sealed class InitiatePaymentCommandHandler(
    IAppDbContext db,
    ICurrentUser currentUser,
    IPaymentGateway paymentGateway)
    : IRequestHandler<InitiatePaymentCommand, Result<PaymentSessionResponse>>
{
    public async Task<Result<PaymentSessionResponse>> Handle(
        InitiatePaymentCommand request,
        CancellationToken ct)
    {
        // 1. Must be logged in — we need to know who's paying
        if (!currentUser.IsAuthenticated)
            return PaymentErrors.Unauthenticated;

        // 2. Load the order
        //    IgnoreQueryFilters because attendees have no TenantId
        //    We check AttendeeId manually to ensure they own this order
        var order = await db.Orders
            .IgnoreQueryFilters()
            .Include(o => o.Attendee)   // need email for Stripe
            .FirstOrDefaultAsync(o =>
                o.Id == request.OrderId &&
                o.AttendeeId == currentUser.UserId, ct);

        if (order is null)
            return OrderErrors.NotFound;

        // 3. Only pending orders can be paid
        //    Paid orders don't need payment, cancelled/expired orders can't be paid
        if (order.OrderStatus != OrderStatus.Pending)
            return PaymentErrors.OrderNotPayable;

        // 4. Check if we already created a payment session for this order
        //    This handles the "double click Pay button" scenario
        //    If the attendee already has a pending payment, don't create another one
        var existingPayment = await db.Payments
            .IgnoreQueryFilters()
            .AnyAsync(p =>
                p.OrderId == order.Id &&
                p.PaymentStatus == PaymentStatus.Pending, ct);

        if (existingPayment)
            return PaymentErrors.PaymentAlreadyInitiated;

        // 5. Tell Stripe "I need to collect this amount from this person"
        //    Stripe creates a PaymentIntent and returns:
        //    - ProviderReferenceId: Stripe's id for this payment (pi_...)
        //    - PaymentUrl: where to send the user to enter their card
        var sessionResult = await paymentGateway.CreatePaymentSessionAsync(
            order.Id,
            order.OrderNumber,
            order.Total,
            order.Attendee.Email,order.Attendee.Name,order.Attendee.PhoneNumber,
            ct);

        // 6. Record that we initiated a payment attempt
        //    Status is Pending — it becomes Succeeded/Failed when Stripe sends the webhook
        var paymentResult = Payment.Initiate(
            Guid.NewGuid(),
            order.TenantId,
            order.Id,
            order.Total,
            sessionResult.ProviderReferenceId); // stored so webhook handler can find this record

        if (paymentResult.IsError)
            return paymentResult.Errors!;

        await db.Payments.AddAsync(paymentResult.Value, ct);
        await db.SaveChangesAsync(ct);

        // 7. Return the payment URL — client will redirect the user there
        return new PaymentSessionResponse(
            sessionResult.PaymentUrl,
            order.OrderNumber);
    }
}

