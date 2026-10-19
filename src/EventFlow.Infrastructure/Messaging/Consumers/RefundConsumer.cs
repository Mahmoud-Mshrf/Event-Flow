using System.Globalization;
using EventFlow.Application.Common.Helpers;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Messages;
using EventFlow.Domain.Payments.Enums;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventFlow.Infrastructure.Messaging.Consumers;

public sealed class RefundConsumer(
    IPaymentGateway paymentGateway,
    IAppDbContext db,
    IPublishEndpoint publishEndpoint,
    IDateTimeProvider dateTimeProvider,
    ILogger<RefundConsumer> logger)
    : IConsumer<RefundRequestedMessage>
{
    public async Task Consume(ConsumeContext<RefundRequestedMessage> context)
    {
        var msg = context.Message;
        var ct = context.CancellationToken;

        // ── 1. Load the payment ───────────────────────────────────────
        // IgnoreQueryFilters: a consumer has no tenant context
        var payment = await db.Payments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == msg.PaymentId, ct);

        if (payment is null)
        {
            logger.LogWarning(
                "RefundConsumer: payment {PaymentId} not found. Skipping.",
                msg.PaymentId);
            return;
        }

        // ── 2. Idempotency: a redelivered message must not refund twice ─
        if (payment.PaymentStatus == PaymentStatus.Refunded)
        {
            logger.LogInformation(
                "RefundConsumer: payment {PaymentId} already refunded. Skipping.",
                msg.PaymentId);
            return;
        }

        // Only a succeeded payment has money to give back
        if (payment.PaymentStatus != PaymentStatus.Succeeded)
        {
            logger.LogWarning(
                "RefundConsumer: payment {PaymentId} is {Status}, not refundable. Skipping.",
                msg.PaymentId, payment.PaymentStatus);
            return;
        }

        // ── 3. Call Paymob ────────────────────────────────────────────
        // Throws on failure, so MassTransit retries this consumer only
        await paymentGateway.RefundPaymentAsync(
            msg.ProviderTransactionId,
            msg.Amount,
            ct);

        // ── 4. Record the refund ──────────────────────────────────────
        // Status was verified as Succeeded above, so this cannot be rejected
        payment.MarkRefunded(dateTimeProvider.UtcNow);
        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "Refund recorded for payment {PaymentId}, transaction {TransactionId}.",
            payment.Id, msg.ProviderTransactionId);

        // ── 5. Email: separate retry scope ────────────────────────────
        // If SMTP fails, only EmailConsumer retries. Paymob is not called again.
        var amount = msg.Amount.ToString("F2", CultureInfo.InvariantCulture);

        await publishEndpoint.Publish(new EmailMessage
        {
            To = msg.AttendeeEmail,
            Subject = $"Your refund for {msg.EventName} is on its way",
            IsHtml = true,
            Body = $"""
                <h2>Your refund has been initiated</h2>
                <p>Hi {EmailHtml.Encode(msg.AttendeeName)},</p>
                <p>Because <strong>{EmailHtml.Encode(msg.EventName)}</strong> was
                cancelled, we've refunded your payment.</p>

                <table style="border-collapse:collapse;width:100%;margin:16px 0;">
                    <tr>
                        <td style="padding:8px;border:1px solid #ddd;"><strong>Order Number</strong></td>
                        <td style="padding:8px;border:1px solid #ddd;">{EmailHtml.Encode(msg.OrderNumber)}</td>
                    </tr>
                    <tr>
                        <td style="padding:8px;border:1px solid #ddd;"><strong>Refund Amount</strong></td>
                        <td style="padding:8px;border:1px solid #ddd;">{amount} EGP</td>
                    </tr>
                </table>

                <p>Depending on your bank, it can take 5-7 business days to appear
                on your statement.</p>
                <p>We apologize for the inconvenience.</p>
                """
        }, ct);
    }
}