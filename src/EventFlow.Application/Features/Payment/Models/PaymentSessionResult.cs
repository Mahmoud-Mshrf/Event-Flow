namespace EventFlow.Application.Features.Payment.Models;

// What comes back from CreatePaymentSessionAsync
public sealed record PaymentSessionResult(
    string ProviderReferenceId, // stored on your Payment entity for later lookup
    string PaymentUrl);         // sent to the client so they can redirect to payment page
