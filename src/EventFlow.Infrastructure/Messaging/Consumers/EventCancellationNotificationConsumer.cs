using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EventFlow.Infrastructure.Messaging.Consumers;

public sealed class EventCancellationNotificationConsumer(
    IEmailSender emailSender,
    ILogger<EventCancellationNotificationConsumer> logger,CancellationToken cancellationToken)
    : IConsumer<EventCancelledMessage>
{
    public async Task Consume(ConsumeContext<EventCancelledMessage> context)
    {
        var msg = context.Message;

        var subject = $"Update regarding your order for {msg.EventName}";

        var body = msg.WasPaid
            ? $"""
               Dear {msg.AttendeeName},

               We regret to inform you that {msg.EventName} has been cancelled.

               Your order ({msg.OrderNumber}) has been affected and your ticket(s) 
               have been cancelled. Please contact us to arrange your refund.

               We sincerely apologize for the inconvenience.
               """
            : $"""
               Dear {msg.AttendeeName},

               We regret to inform you that {msg.EventName} has been cancelled.

               Your pending order ({msg.OrderNumber}) has been automatically cancelled 
               and no payment was taken.

               We sincerely apologize for the inconvenience.
               """;
        await emailSender.SendAsync(msg.AttendeeEmail,subject, body,false,cancellationToken);

        logger.LogInformation(
            "Event cancellation email sent to {Email} for order {OrderNumber}.",
            msg.AttendeeEmail,
            msg.OrderNumber);
    }
}