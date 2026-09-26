using System.Text.Json.Serialization;

namespace EventFlow.Infrastructure.PaymentGateways.Paymob.Models;

internal sealed class PaymobTransactionObj
{
    [JsonPropertyName("id")]
    public long Id { get; set; }                            // transaction ID

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("amount_cents")]
    public int AmountCents { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = null!;

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = null!;

    [JsonPropertyName("error_occured")]
    public bool ErrorOccured { get; set; }

    [JsonPropertyName("has_parent_transaction")]
    public bool HasParentTransaction { get; set; }

    [JsonPropertyName("integration_id")]
    public int IntegrationId { get; set; }

    [JsonPropertyName("is_3d_secure")]
    public bool Is3dSecure { get; set; }

    [JsonPropertyName("is_auth")]
    public bool IsAuth { get; set; }

    [JsonPropertyName("is_capture")]
    public bool IsCapture { get; set; }

    [JsonPropertyName("is_refunded")]
    public bool IsRefunded { get; set; }

    [JsonPropertyName("is_standalone_payment")]
    public bool IsStandalonePayment { get; set; }

    [JsonPropertyName("is_voided")]
    public bool IsVoided { get; set; }

    [JsonPropertyName("pending")]
    public bool Pending { get; set; }

    [JsonPropertyName("owner")]
    public long Owner { get; set; }

    [JsonPropertyName("order")]
    public PaymobOrderRef? Order { get; set; }

    [JsonPropertyName("source_data")]
    public PaymobSourceData? SourceData { get; set; }
}
