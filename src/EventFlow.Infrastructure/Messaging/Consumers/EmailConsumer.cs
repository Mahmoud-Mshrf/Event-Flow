using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace EventFlow.Infrastructure.Messaging.Consumers;

public sealed class EmailConsumer(
    IEmailSender emailSender,
    ILogger<EmailConsumer> logger,CancellationToken cancellationToken)
    : IConsumer<EmailMessage>
{
    public async Task Consume(ConsumeContext<EmailMessage> context)
    {
        var msg = context.Message;

        await emailSender.SendAsync(msg.To, msg.Subject, msg.Body,msg.IsHtml,cancellationToken);

        logger.LogInformation(
            "Email sent to {To} with subject '{Subject}'.",
            msg.To,
            msg.Subject);
    }
}