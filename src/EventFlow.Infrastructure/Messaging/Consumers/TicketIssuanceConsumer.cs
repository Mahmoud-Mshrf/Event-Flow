using EventFlow.Application.Common.Helpers;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Messages;
using EventFlow.Domain.Tickets;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace EventFlow.Infrastructure.Messaging.Consumers;

public sealed class TicketIssuanceConsumer(
    IAppDbContext db,
    IQrCodeService qrCodeService,
    HybridCache cache) : IConsumer<PaymentConfirmedMessage>
{
    public async Task Consume(ConsumeContext<PaymentConfirmedMessage> context)
    {
        var message = context.Message;
        var ct = context.CancellationToken;

        // Idempotency: check if tickets already exist for this order
        // The consumer might be called twice if RabbitMQ retries
        var alreadyIssued = await db.Tickets
            .IgnoreQueryFilters()
            .AnyAsync(t => t.OrderId == message.OrderId, ct);

        if (alreadyIssued)
            return; // already done — idempotent

        var issuedTickets = new List<Ticket>();

        foreach (var item in message.Items)
        {
            for (var i = 0; i < item.Quantity; i++)
            {
                var ticketId = Guid.NewGuid();
                var qrPayload = qrCodeService.GeneratePayload(ticketId);
                var ticketNumber = TicketNumberGenerator.Generate();

                var ticketResult = Ticket.Create(
                    ticketId,
                    message.OrderId,   // we need EventId — should add to message
                    message.TenantId,
                    message.AttendeeId,
                    item.TicketTypeId,
                    message.OrderId,
                    ticketNumber,
                    qrPayload);

                if (ticketResult.IsError) continue;
                issuedTickets.Add(ticketResult.Value);
            }
        }

        await db.Tickets.AddRangeAsync(issuedTickets, ct);
        await db.SaveChangesAsync(ct);

        await cache.RemoveByTagAsync($"attendee-{message.AttendeeId}-tickets", ct);
    }
}
