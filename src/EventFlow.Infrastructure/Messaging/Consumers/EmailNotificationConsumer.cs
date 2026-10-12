using EventFlow.Application.Common.Helpers;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Messages;
using MassTransit;

namespace EventFlow.Infrastructure.Messaging.Consumers;

public sealed class EmailNotificationConsumer(
    IEmailSender emailSender) : IConsumer<PaymentConfirmedMessage>
{
    public async Task Consume(ConsumeContext<PaymentConfirmedMessage> context)
    {
        var message = context.Message;

        var tickets = message.Items
            .SelectMany(item => Enumerable.Range(0, item.Quantity)
                .Select(_ => new IssuedTicketInfo(
                    TicketNumberGenerator.Generate(), // placeholder — real number comes from DB
                    item.TicketTypeName,
                    string.Empty)))  // QR not needed in email — ticket is in the app
            .ToList();

        await emailSender.SendTicketConfirmationAsync(
            message.AttendeeEmail,
            message.AttendeeName,
            message.EventName,
            message.OrderNumber,
            tickets,
            context.CancellationToken);
    }
}