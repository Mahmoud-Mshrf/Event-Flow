using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Payments.Models;
using EventFlow.Infrastructure.PaymentGateways.Paymob.Models;
using Microsoft.Extensions.Options;

namespace EventFlow.Infrastructure.PaymentGateways.Paymob.Service;

public sealed class PaymobPaymentGateway(
    IOptions<PaymobSettings> settings,
    IHttpClientFactory httpClientFactory) : IPaymentGateway
{
    private readonly PaymobSettings _settings = settings.Value;

    // ══════════════════════════════════════════════════════════
    // CREATE INTENTION — modern single-step API
    // ══════════════════════════════════════════════════════════
    public async Task<PaymentSessionResult> CreatePaymentSessionAsync(
        Guid orderId,
        string orderNumber,
        decimal amount,
        string customerEmail,
        string customerFirstName,
        string customerPhone,
        CancellationToken ct)
    {
        var client = httpClientFactory.CreateClient("Paymob");

        var intentionRequest = new PaymobIntentionRequest
        {
            // Paymob uses integer cents — multiply by 100
            // 50 EGP = 5000 cents
            Amount = (int)(amount * 100),

            Currency = "EGP",

            // Which payment methods to offer — card integration ID
            // You can add more integration IDs here (Fawry: add its ID, etc.)
            PaymentMethods = _settings.CardIntegrationIds,

            Items =
            [
                new PaymobItem
                {
                    Name = $"EventFlow Tickets — {orderNumber}",
                    Amount = (int)(amount * 100),
                    Description = "Event ticket purchase",
                    Quantity = 1
                }
            ],

            BillingData = new PaymobBillingData
            {
                Email = customerEmail,
                FirstName = customerFirstName,
                LastName = "NA",            // we only collect one name field
                PhoneNumber = customerPhone
            },

            // This comes back in the webhook as order.merchant_order_id
            // It's how you link the Paymob callback to your order
            SpecialReference = orderNumber,

            // Where Paymob POSTs full transaction details after payment
            NotificationUrl = _settings.NotificationUrl,

            // Where Paymob redirects the attendee after payment completes
            // (success or failure — check the query params to know which)
            RedirectionUrl = _settings.RedirectionUrl
        };

        // POST to /v1/intention
        // Authorization: Token sk_test_...   ← note "Token " prefix, not "Bearer "
        var response = await client.PostAsJsonAsync(
            $"{_settings.BaseUrl}/v1/intention/",
            intentionRequest,
            ct);

        response.EnsureSuccessStatusCode();

        var intention = await response.Content
            .ReadFromJsonAsync<PaymobIntentionResponse>(ct);

        // Build the Unified Checkout URL
        // Attendee goes here to complete payment
        var checkoutUrl =
            $"{_settings.BaseUrl}/unifiedcheckout/" +
            $"?publicKey={_settings.PublicKey}" +
            $"&clientSecret={intention!.ClientSecret}";

        return new PaymentSessionResult(
            // Store the intention ID as the provider reference
            // It comes back in the webhook and lets us find this Payment record
            ProviderReferenceId: orderNumber,   // we use orderNumber since it's in special_reference
            PaymentUrl: checkoutUrl);
    }

    // ══════════════════════════════════════════════════════════
    // WEBHOOK VERIFICATION — using Paymob's documented HMAC method
    // ══════════════════════════════════════════════════════════

    public PaymentWebhookEvent? ParseAndVerifyWebhook(WebhookRequest request)
    {
        try
        {
            // Paymob-specific: HMAC arrives as ?hmac= query parameter
            // This knowledge stays here — never leaks to controller or handler
            if (!request.QueryParameters.TryGetValue("hmac", out var hmacFromQuery)
                || string.IsNullOrWhiteSpace(hmacFromQuery))
                return null;

            var callback = JsonSerializer.Deserialize<PaymobTransactionCallback>(
                request.RawBody);
            // Only handle TRANSACTION type callbacks
            if (callback?.Obj is null || callback.Type != "TRANSACTION")
                return null;

            var obj = callback.Obj;

            // ── HMAC VERIFICATION ──────────────────────────────────
            // Paymob's documented HMAC calculation (from their docs):
            // Step 1: Take these specific fields IN THIS EXACT ORDER
            // Step 2: Concatenate their string values
            // Step 3: HMAC-SHA512 with your HMAC secret
            // Step 4: Compare with ?hmac= query parameter
            var concatenated = string.Concat(
                obj.AmountCents.ToString(),
                obj.CreatedAt,
                obj.Currency,
                obj.ErrorOccured.ToString().ToLower(),
                obj.HasParentTransaction.ToString().ToLower(),
                obj.Id.ToString(),
                obj.IntegrationId.ToString(),
                obj.Is3dSecure.ToString().ToLower(),
                obj.IsAuth.ToString().ToLower(),
                obj.IsCapture.ToString().ToLower(),
                obj.IsRefunded.ToString().ToLower(),
                obj.IsStandalonePayment.ToString().ToLower(),
                obj.IsVoided.ToString().ToLower(),
                obj.Order?.Id.ToString(),
                obj.Owner.ToString(),
                obj.Pending.ToString().ToLower(),
                obj.SourceData?.Pan ?? string.Empty,
                obj.SourceData?.SubType ?? string.Empty,
                obj.SourceData?.Type ?? string.Empty,
                obj.Success.ToString().ToLower());

            var keyBytes = Encoding.UTF8.GetBytes(_settings.HmacSecret);
            var messageBytes = Encoding.UTF8.GetBytes(concatenated);
            using var hmac = new HMACSHA512(keyBytes);
            var hash = hmac.ComputeHash(messageBytes);
            var computedHmac = Convert.ToHexString(hash).ToLower();

            if (!computedHmac.Equals(hmacFromQuery, StringComparison.OrdinalIgnoreCase))
                return null;
            // ── BUILD PaymentWebhookEvent ──────────────────────────
            // merchant_order_id = the special_reference we set = your orderNumber
            bool isSucceeded = obj.Success && !obj.IsVoided
                && !obj.IsRefunded && !obj.Pending;
            bool isFailed = obj.ErrorOccured
                || (!obj.Success && !obj.Pending);
            
            return new PaymentWebhookEvent(
                ProviderEventId: obj.Id.ToString(),
                // special_reference comes back as merchant_order_id — our orderNumber
                ProviderReferenceId: obj.Order?.MerchantOrderId ?? string.Empty,
                EventType: callback.Type,
                IsPaymentSucceeded: isSucceeded,
                IsPaymentFailed: isFailed,
                FailureReason: obj.ErrorOccured
                    ? "Payment was declined." : null,
                Amount: obj.AmountCents / 100m);
        }
        catch
        {
            // Any parsing error = invalid payload = reject
            return null;
        }
    }

}