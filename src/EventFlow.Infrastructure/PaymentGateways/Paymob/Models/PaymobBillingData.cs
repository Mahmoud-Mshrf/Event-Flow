using System.Text.Json.Serialization;

namespace EventFlow.Infrastructure.PaymentGateways.Paymob.Models;

internal sealed class PaymobBillingData
{
    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = "NA";

    [JsonPropertyName("last_name")]
    public string LastName { get; set; } = "NA";

    [JsonPropertyName("email")]
    public string Email { get; set; } = null!;

    [JsonPropertyName("phone_number")]
    public string PhoneNumber { get; set; } = null!;

    [JsonPropertyName("apartment")]
    public string Apartment { get; set; } = "NA";

    [JsonPropertyName("floor")]
    public string Floor { get; set; } = "NA";

    [JsonPropertyName("street")]
    public string Street { get; set; } = "NA";

    [JsonPropertyName("building")]
    public string Building { get; set; } = "NA";

    [JsonPropertyName("city")]
    public string City { get; set; } = "NA";

    [JsonPropertyName("country")]
    public string Country { get; set; } = "EG";

    [JsonPropertyName("state")]
    public string State { get; set; } = "NA";

    [JsonPropertyName("postal_code")]
    public string PostalCode { get; set; } = "NA";
}