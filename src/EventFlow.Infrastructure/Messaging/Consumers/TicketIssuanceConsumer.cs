using System.Net;
using EventFlow.Application.Common.Helpers;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Messages;
using EventFlow.Domain.Tickets;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventFlow.Infrastructure.Messaging.Consumers;

public sealed class TicketIssuanceConsumer(
    IAppDbContext db,
    IQrCodeService qrCodeService,
    IPublishEndpoint publishEndpoint,
    ILogger<TicketIssuanceConsumer> logger)
    : IConsumer<PaymentConfirmedMessage>
{
    public async Task Consume(ConsumeContext<PaymentConfirmedMessage> context)
    {
        var msg = context.Message;
        var ct = context.CancellationToken;

        // ── Idempotency check ─────────────────────────────────────────
        var alreadyIssued = await db.Tickets
            .IgnoreQueryFilters()
            .AnyAsync(t => t.OrderId == msg.OrderId, ct);

        if (alreadyIssued)
        {
            logger.LogInformation(
                "Tickets already issued for order {OrderId}. Skipping.",
                msg.OrderId);
            return;
        }

        // ── Issue tickets from the message items (no DB read needed) ──
        // We keep the type name next to each ticket for the email table
        var issued = new List<(Ticket Ticket, string TypeName)>();

        foreach (var item in msg.Items)
        {
            for (var i = 0; i < item.Quantity; i++)
            {
                // The QR signs the TicketId, the TicketNumber is only a label
                var ticketId = Guid.NewGuid();
                var ticketNumber = TicketNumberGenerator.Generate();
                var qrCode = qrCodeService.GeneratePayload(ticketId);

                var ticket = Ticket.Create(
                    id: ticketId,
                    eventId: msg.EventId,
                    tenantId: msg.TenantId,
                    attendeeId: msg.AttendeeId,
                    ticketTypeId: item.TicketTypeId,
                    orderId: msg.OrderId,
                    ticketNumber: ticketNumber,
                    qrCode: qrCode);

                issued.Add((ticket.Value, item.TicketTypeName));
            }
        }

        await db.Tickets.AddRangeAsync(issued.Select(x => x.Ticket), ct);
        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "Issued {Count} tickets for order {OrderId}.",
            issued.Count, msg.OrderId);

        // ── Publish HTML email — separate retry scope ─────────────────
        await publishEndpoint.Publish(new EmailMessage
        (
            To : msg.AttendeeEmail,
            Subject : $"Your tickets for {msg.EventName}",
            IsHtml : true,
            Body : BuildTicketsEmailBody(
                msg.AttendeeName,
                msg.EventName,
                msg.OrderNumber,
                issued.Select(x => (x.Ticket.TicketNumber, x.TypeName)))
        ), ct);
    }

    private static string BuildTicketsEmailBody(
        string attendeeName,
        string eventName,
        string orderNumber,
        IEnumerable<(string TicketNumber, string TypeName)> tickets)
    {
        // Attendee and event names are user/organizer-controlled.
        // Encode them so nobody can inject markup into an email we send.
        var name = WebUtility.HtmlEncode(attendeeName);
        var evt = WebUtility.HtmlEncode(eventName);
        var order = WebUtility.HtmlEncode(orderNumber);

        var ticketRows = string.Join("", tickets.Select(t => $"""
            <tr>
                <td style="padding:8px;border:1px solid #ddd;">{WebUtility.HtmlEncode(t.TicketNumber)}</td>
                <td style="padding:8px;border:1px solid #ddd;">{WebUtility.HtmlEncode(t.TypeName)}</td>
            </tr>
            """));

        return $"""
            <h2>Your tickets are confirmed!</h2>
            <p>Hi {name},</p>
            <p>Thank you for your purchase. Your tickets for <strong>{evt}</strong> are ready.</p>
            <p><strong>Order Number:</strong> {order}</p>

            <table style="border-collapse:collapse;width:100%;margin:16px 0;">
                <thead>
                    <tr style="background:#1a1a2e;color:white;">
                        <th style="padding:8px;text-align:left;">Ticket Number</th>
                        <th style="padding:8px;text-align:left;">Type</th>
                    </tr>
                </thead>
                <tbody>{ticketRows}</tbody>
            </table>

            <p>Present your ticket QR code at the event entrance.
            You can view your tickets anytime in the EventFlow app.</p>
            """;
    }
}