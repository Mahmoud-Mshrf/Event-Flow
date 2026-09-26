using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Payments.Models;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Payments;
using EventFlow.Domain.Payments.Events;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace EventFlow.Application.Features.Payments.Commands.HandleWebhook;

public sealed class HandleWebhookCommandHandler(
    IAppDbContext db,
    IPaymentGateway paymentGateway,
    HybridCache cache)
    : IRequestHandler<HandleWebhookCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(
        HandleWebhookCommand request,
        CancellationToken ct)
    {
        // ── STEP 1: Verify and parse ──────────────────────────
        // Gateway reads from wherever the signature lives
        // Paymob: QueryParameters["hmac"]
        // Stripe: Headers["Stripe-Signature"]
        // Handler knows nothing about either
        var webhookEvent = paymentGateway.ParseAndVerifyWebhook(request.Request);

        if (webhookEvent is null)
            return PaymentErrors.InvalidSignature;

        // ── STEP 2: Idempotency ───────────────────────────────
        // Try to record this event — unique index rejects duplicates atomically
        db.ProcessedWebhookEvents.Add(
            ProcessedWebhookEvent.Create(
                webhookEvent.ProviderEventId,
                webhookEvent.EventType));

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            // Already processed — return 200 silently
            // Never return an error code — provider would retry
            return Result.Success;
        }

        // ── STEP 3: Route to correct handler ─────────────────
        if (webhookEvent.IsPaymentSucceeded)
            return await HandlePaymentSucceeded(webhookEvent, ct);

        if (webhookEvent.IsPaymentFailed)
            return await HandlePaymentFailed(webhookEvent, ct);

        // Unknown event type — acknowledge and ignore
        return Result.Success;
    }

    private async Task<Result<Success>> HandlePaymentSucceeded(
        PaymentWebhookEvent webhookEvent,
        CancellationToken ct)
    {
        // ProviderReferenceId is whatever the gateway stored at initiation:
        // Paymob  → orderNumber (from special_reference → merchant_order_id)
        // Stripe  → PaymentIntent.Id
        // Handler uses it for lookup without knowing which provider is active
        var payment = await db.Payments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p =>
                p.ProviderReferenceId == webhookEvent.ProviderReferenceId, ct);

        if (payment is null)
            return PaymentErrors.PaymentNotFound;

        // Domain rule: only Pending payments can succeed
        // Also acts as second idempotency layer if ProcessedWebhookEvent somehow failed
        var paymentResult = payment.MarkSucceeded(DateTime.UtcNow);
        if (paymentResult.IsError)
            return Result.Success;  // already succeeded — safe no-op

        // Load the order with its items
        // Items needed by PaymentSucceededDomainEvent handler for ticket issuance
        var order = await db.Orders
            .IgnoreQueryFilters()
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == payment.OrderId, ct);

        if (order is null)
            return PaymentErrors.OrderNotFound;

        var orderResult = order.MarkAsPaid(DateTime.UtcNow);
        if (orderResult.IsError)
            return Result.Success;  // already paid — safe no-op

        // Raise domain event — dispatched after SaveChangesAsync
        // → PaymentSucceededDomainEventHandler creates tickets and sends email
        payment.AddDomainEvent(new PaymentSucceededDomainEvent(
            payment.Id,
            order.Id,
            order.TenantId));

        // Persist both payment and order status changes atomically
        await db.SaveChangesAsync(ct);

        // Invalidate order cache — attendee should now see "Paid" status
        await cache.RemoveByTagAsync($"attendee-{order.AttendeeId}-orders", ct);
        await cache.RemoveByTagAsync($"event-{order.EventId}-orders", ct);

        return Result.Success;
    }

    private async Task<Result<Success>> HandlePaymentFailed(
        PaymentWebhookEvent webhookEvent,
        CancellationToken ct)
    {
        var payment = await db.Payments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p =>
                p.ProviderReferenceId == webhookEvent.ProviderReferenceId, ct);

        // If not found — ignore silently
        // Could be a race condition or a test event — not worth erroring on
        if (payment is null)
            return Result.Success;

        var result = payment.MarkFailed(
            webhookEvent.FailureReason ?? "Payment failed.",
            DateTime.UtcNow);

        // If already failed — idempotent, ignore
        if (result.IsError)
            return Result.Success;

        await db.SaveChangesAsync(ct);

        return Result.Success;
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex) =>
        ex.InnerException is SqlException sqlEx &&
        (sqlEx.Number == 2627 || sqlEx.Number == 2601);
}