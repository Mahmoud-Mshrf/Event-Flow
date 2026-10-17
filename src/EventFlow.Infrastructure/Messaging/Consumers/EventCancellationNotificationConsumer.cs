using EventFlow.Application.Common.Helpers;
using EventFlow.Application.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EventFlow.Infrastructure.Messaging.Consumers;

public sealed class EventCancellationNotificationConsumer(
    IPublishEndpoint publishEndpoint,
    ILogger<EventCancellationNotificationConsumer> logger)
    : IConsumer<EventCancelledMessage>
{
    public async Task Consume(ConsumeContext<EventCancelledMessage> context)
    {
        var msg = context.Message;

        var name = EmailHtml.Encode(msg.AttendeeName);
        var evt = EmailHtml.Encode(msg.EventName);
        var order = EmailHtml.Encode(msg.OrderNumber);

        var details = msg.WasPaid
            ? $"""
               <p>Your order <strong>{order}</strong> has been affected and your
               ticket(s) are no longer valid.</p>
               <p>A full refund is being processed automatically. You'll get a
               separate email once it has been initiated.</p>
               """
            : $"""
               <p>Your pending order <strong>{order}</strong> was cancelled
               automatically. No payment was taken.</p>
               """;

        await publishEndpoint.Publish(new EmailMessage
        {
            To = msg.AttendeeEmail,
            Subject = $"{msg.EventName} has been cancelled",
            IsHtml = true,
            Body = $"""
                <h2>Event cancelled</h2>
                <p>Hi {name},</p>
                <p>We're sorry to let you know that <strong>{evt}</strong>
                has been cancelled.</p>
                {details}
                <p>We apologize for the inconvenience.</p>
                """
        }, context.CancellationToken);

        logger.LogInformation(
            "Event cancellation email queued for {Email}, order {OrderNumber}.",
            msg.AttendeeEmail, msg.OrderNumber);
    }
}