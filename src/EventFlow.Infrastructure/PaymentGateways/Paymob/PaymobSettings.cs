namespace EventFlow.Infrastructure.PaymentGateways.Paymob;

public sealed class PaymobSettings
{
    // From Paymob dashboard → Settings → API Keys
    public const string SectionName = "Paymob";
    public string SecretKey { get; init; } = null!;       // Token sk_test_...
    public string PublicKey { get; init; } = null!;        // pk_test_...
    public string HmacSecret { get; init; } = null!;       // For webhook verification

    // From Paymob dashboard → Settings → Payment Integrations
    // Each payment method has its own integration ID
    // For MVP: just card payment
    public List<int> CardIntegrationIds { get; init; }=[];

    // Where Paymob redirects the attendee after payment completes
    public string RedirectionUrl { get; init; } = null!;   // your frontend success/cancel page

    // Your webhook endpoint — where Paymob POSTs transaction details
    public string NotificationUrl { get; init; } = null!;  // https://yourapp.com/api/payments/webhook

    // Paymob base URL — different per region
    // Egypt: https://accept.paymob.com
    public string BaseUrl { get; init; } = "https://accept.paymob.com";
}

