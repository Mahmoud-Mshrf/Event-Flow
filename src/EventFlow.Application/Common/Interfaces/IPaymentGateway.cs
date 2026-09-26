using EventFlow.Application.Features.Payment.Models;

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
}

