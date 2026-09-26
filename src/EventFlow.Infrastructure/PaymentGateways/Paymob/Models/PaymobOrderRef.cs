using System.Text.Json.Serialization;

namespace EventFlow.Infrastructure.PaymentGateways.Paymob.Models;

internal sealed class PaymobOrderRef
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    // This is the special_reference you passed when creating the intention
    // = your order number
    [JsonPropertyName("merchant_order_id")]
    public string? MerchantOrderId { get; set; }
}
