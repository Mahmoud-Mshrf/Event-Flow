using System.Text.Json.Serialization;

namespace EventFlow.Infrastructure.PaymentGateways.Paymob.Models;

// Request body for POST /v1/intention
internal sealed class PaymobIntentionRequest
{
    [JsonPropertyName("amount")]
    public int Amount { get; set; }                         // in cents (EGP × 100)

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "EGP";

    [JsonPropertyName("payment_methods")]
    public List<int> PaymentMethods { get; set; } = [];    // integration IDs

    [JsonPropertyName("items")]
    public List<PaymobItem> Items { get; set; } = [];

    [JsonPropertyName("billing_data")]
    public PaymobBillingData? BillingData { get; set; }

    [JsonPropertyName("special_reference")]
    public string? SpecialReference { get; set; }           // your order number — comes back in webhook

    [JsonPropertyName("notification_url")]
    public string? NotificationUrl { get; set; }            // your webhook endpoint

    [JsonPropertyName("redirection_url")]
    public string? RedirectionUrl { get; set; }             // where attendee goes after payment
}
