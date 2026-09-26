using EventFlow.Application.Features.Payments.Commands.HandleWebhook;
using EventFlow.Application.Features.Payments.Commands.InitiatePayment;
using EventFlow.Application.Features.Payments.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.Api.Controllers;

[Route("api/payments")]
public sealed class PaymentsController(ISender sender) : ApiController
{
    // ── Initiate payment ──────────────────────────────────────
    // Called by the attendee after creating an order
    // Returns a URL to redirect the attendee to for payment
    [HttpPost("initiate")]
    [Authorize]
    [ProducesResponseType(typeof(PaymentSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [EndpointSummary("Initiate a payment session for a pending order.")]
    [EndpointName("InitiatePayment")]
    public async Task<ActionResult> Initiate(
        [FromBody] InitiatePaymentCommand command,
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.Match(response => Ok(response), Problem);
    }

    // ── Webhook ───────────────────────────────────────────────
    // Called by Paymob (or any gateway) after payment completes
    // MUST be AllowAnonymous — gateway has no JWT
    // Security is enforced inside the handler via signature verification
    // Controller is 100% provider-agnostic — no Stripe headers, no Paymob query params
    [HttpPost("webhook")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("Payment gateway webhook endpoint.")]
    [EndpointName("HandleWebhook")]
    public async Task<ActionResult> Webhook(CancellationToken ct)
    {
        // Read raw body before ASP.NET Core does anything with it
        // Body must be raw for signature verification — any parsing may alter bytes
        using var reader = new StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync(ct);

        // Collect ALL headers — gateway picks what it needs
        // Paymob doesn't use headers for auth
        // Stripe uses "Stripe-Signature" header
        // Future gateways may use different headers
        // Controller knows nothing about which one is active
        var headers = Request.Headers
            .ToDictionary(
                h => h.Key,
                h => h.Value.ToString(),
                StringComparer.OrdinalIgnoreCase);

        // Collect ALL query parameters — gateway picks what it needs
        // Paymob sends HMAC as ?hmac= query parameter
        // Stripe doesn't use query params for auth
        // Controller knows nothing about this
        var queryParams = Request.Query
            .ToDictionary(
                q => q.Key,
                q => q.Value.ToString(),
                StringComparer.OrdinalIgnoreCase);

        var webhookRequest = new WebhookRequest(rawBody, headers, queryParams);

        // Send to handler — result intentionally ignored for HTTP response
        // We always return 200 regardless of what happened inside
        await sender.Send(new HandleWebhookCommand(webhookRequest), ct);

        // Always 200 OK:
        // - Duplicate event → 200 (already handled, no need to retry)
        // - Invalid signature → 200 (we log it, no retry needed)
        // - Successful processing → 200 ✅
        //
        // Only case for non-200: genuine infrastructure failure (DB down)
        // where you want the gateway to retry. For MVP, 200 always is correct.
        return Ok();
    }
}