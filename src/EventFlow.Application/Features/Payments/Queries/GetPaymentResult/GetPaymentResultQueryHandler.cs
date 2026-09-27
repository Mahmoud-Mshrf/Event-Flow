using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Orders;
using EventFlow.Domain.Orders.Enums;
using EventFlow.Domain.Payments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Payments.Queries.GetPaymentResult;

public sealed class GetPaymentResultQueryHandler(
    IAppDbContext db,
    ICurrentUser currentUser,
    IPaymentGateway paymentGateway)
    : IRequestHandler<GetPaymentResultQuery, Result<PaymentResultDto>>
{
    public async Task<Result<PaymentResultDto>> Handle(
        GetPaymentResultQuery request,
        CancellationToken ct)
    {
        // 1. Verify the redirect params haven't been tampered with
        //    Same HMAC mechanism as the webhook
        //    If verification fails — don't trust success/failure param
        var isVerified = paymentGateway.VerifyRedirectParams(
            request.AllQueryParams,
            request.Hmac ?? string.Empty);

        if (!isVerified)
            return PaymentErrors.InvalidSignature;

        // 2. Look up the order by order number
        //    Source of truth is our DB — not the redirect params
        //    Webhook may have already processed it by the time attendee is redirected
        var order = await db.Orders
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(o =>
                o.OrderNumber == request.OrderNumber, ct);

        if (order is null)
            return OrderErrors.NotFound;

        // 3. Build response from actual DB state
        //    Never from the redirect's success=true/false param
        //    Webhook might be slightly delayed — order might still be Pending
        //    if the redirect arrives before the webhook is processed
        var isSuccessful = order.OrderStatus == OrderStatus.Paid;

        var message = order.OrderStatus switch
        {
            OrderStatus.Paid       => "Payment successful. Your tickets have been issued.",
            OrderStatus.Pending    => "Payment is being processed. Please check back shortly.",
            OrderStatus.Cancelled  => "This order has been cancelled.",
            OrderStatus.Expired    => "This order has expired.",
            _                      => "Unknown order status."
        };

        return new PaymentResultDto(
            order.OrderNumber,
            isSuccessful,
            order.OrderStatus,
            message);
    }
}