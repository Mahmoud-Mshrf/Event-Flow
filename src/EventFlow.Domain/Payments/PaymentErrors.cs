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
            "You must be logged in to purchase a payment.");
    public static readonly Error OrderNotPayable =
        Error.Unauthorized("Payment.OrderNotPayable",
            "You must be logged in to purchase a payment.");    
    public static readonly Error PaymentAlreadyInitiated =
        Error.Unauthorized("Payment.PaymentAlreadyInitiated",
            "You must be logged in to purchase a payment.");       
}

