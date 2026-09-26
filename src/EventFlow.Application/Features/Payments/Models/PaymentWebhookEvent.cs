namespace EventFlow.Application.Features.Payments.Models;

public sealed record PaymentWebhookEvent(
    string ProviderEventId,      // Stripe: evt_...  used for idempotency check
    string ProviderReferenceId,  // Stripe: pi_...   links to your Payment entity
    string EventType,            // Stripe: "payment_intent.succeeded"
    bool IsPaymentSucceeded,
    bool IsPaymentFailed,
    string? FailureReason,
    decimal Amount);