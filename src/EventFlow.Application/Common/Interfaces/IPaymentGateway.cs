using EventFlow.Application.Features.Payments.Models;

namespace EventFlow.Application.Common.Interfaces;

public interface IPaymentGateway
{
    Task<PaymentSessionResult> CreatePaymentSessionAsync( Guid orderId,
        string orderNumber,
        decimal amount,
        string customerEmail,
        string customerFirstName,
        string customerPhone,
        CancellationToken ct);

    PaymentWebhookEvent? ParseAndVerifyWebhook(WebhookRequest request);

    // Verifies the HMAC on Paymob's redirect query params
    // Returns true if params are legitimate and untampered
    bool VerifyRedirectParams(
        IReadOnlyDictionary<string, string> queryParams,
        string hmac);
}

