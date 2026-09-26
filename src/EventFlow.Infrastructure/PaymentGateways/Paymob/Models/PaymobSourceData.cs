using System.Text.Json.Serialization;

namespace EventFlow.Infrastructure.PaymentGateways.Paymob.Models;

internal sealed class PaymobSourceData
{
    [JsonPropertyName("pan")]
    public string? Pan { get; set; }                        // last 4 digits of card

    [JsonPropertyName("type")]
    public string? Type { get; set; }                       // "card"

    [JsonPropertyName("sub_type")]
    public string? SubType { get; set; }                    // "MasterCard", "Visa"
}