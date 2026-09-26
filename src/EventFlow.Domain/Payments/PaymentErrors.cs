using EventFlow.Domain.Common.Errors;

namespace EventFlow.Domain.Payments;

public static class PaymentErrors
{
    public static readonly Error TenantIdRequired =
        Error.Validation("Payment.TenantIdRequired", "A tenant id is required.");

    public static readonly Error OrderIdRequired =
        Error.Validation("Payment.OrderIdRequired", "An order id is required.");

    public static readonly Error InvalidAmount =
        Error.Validation("Payment.InvalidAmount", "Payment amount must be greater than zero.");

    public static readonly Error MissingProviderReference =
        Error.Validation("Payment.MissingProviderReference", "A provider reference id is required.");

    public static readonly Error CannotTransitionFromNonPending =
        Error.Validation("Payment.CannotTransitionFromNonPending", "Only a pending payment can succeed or fail.");

public static readonly Error Unauthenticated =
        Error.Unauthorized("Payment.Unauthenticated",
            "You must be logged in to initiate a payment.");

    public static readonly Error OrderNotPayable =
        Error.Conflict("Payment.OrderNotPayable",
            "This order cannot be paid. It may already be paid, expired, or cancelled.");

    public static readonly Error PaymentAlreadyInitiated =
        Error.Conflict("Payment.AlreadyInitiated",
            "A payment session already exists for this order. Please complete or wait for it to expire.");

    public static readonly Error InvalidSignature =
        Error.Unauthorized("Payment.InvalidSignature",
            "The webhook request could not be verified.");

    public static readonly Error PaymentNotFound =
        Error.NotFound("Payment.NotFound",
            "No payment record was found for this transaction.");

    public static readonly Error OrderNotFound =
        Error.NotFound("Payment.OrderNotFound",
            "The order associated with this payment could not be found."); 
}

