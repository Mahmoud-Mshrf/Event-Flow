using EventFlow.Domain.Common.Results;
using MediatR;

namespace EventFlow.Application.Features.Payments.Commands.InitiatePayment;

// The attendee has already created an Order
// Now they want to actually pay for it
// This command creates a payment session with Stripe and returns a URL
public sealed record InitiatePaymentCommand(
    Guid OrderId) : IRequest<Result<PaymentSessionResponse>>;

// What we send back to the client
public sealed record PaymentSessionResponse(
    string PaymentUrl,      // client redirects the user here to complete payment
    string OrderNumber);    // for display purposes
