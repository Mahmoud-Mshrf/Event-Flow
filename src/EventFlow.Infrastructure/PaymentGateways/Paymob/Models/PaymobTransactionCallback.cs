using System.Text.Json.Serialization;

namespace EventFlow.Infrastructure.PaymentGateways.Paymob.Models;

// Webhook callback body (POST from Paymob to your notification_url)
internal sealed class PaymobTransactionCallback
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = null!;               // "TRANSACTION"

    [JsonPropertyName("obj")]
    public PaymobTransactionObj Obj { get; set; } = null!;
}
