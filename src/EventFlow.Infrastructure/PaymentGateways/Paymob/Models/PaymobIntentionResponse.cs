using System.Text.Json.Serialization;

namespace EventFlow.Infrastructure.PaymentGateways.Paymob.Models;

// Response from POST /v1/intention
internal sealed class PaymobIntentionResponse
{
    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; set; } = null!;       // used to build checkout URL

    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;                 // intention ID

    [JsonPropertyName("intention_order_id")]
    public long IntentionOrderId { get; set; }              // Paymob's order ID
}
