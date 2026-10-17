using EventFlow.Application.Common.Helpers;
using EventFlow.Application.Messages;
using EventFlow.Application.Settings;
using EventFlow.Domain.Users.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Options;

namespace EventFlow.Application.Features.Authentication.EventHandlers;

internal sealed class EmailConfirmationResentDomainEventHandler(
    IPublishEndpoint publishEndpoint,
    IOptions<VerificationTokenSettings> tokenSettings)
    : INotificationHandler<EmailConfirmationResentDomainEvent>
{
    public async Task Handle(
        EmailConfirmationResentDomainEvent notification,
        CancellationToken ct)
    {
        var hours = tokenSettings.Value.EmailConfirmationExpiryHours;

        await publishEndpoint.Publish(new EmailMessage
        {
            To = notification.Email,
            Subject = "Your new EventFlow confirmation code",
            IsHtml = true,
            Body = $"""
                <h2>Your new confirmation code</h2>
                <p>Here is your new code:</p>
                {EmailHtml.Code(notification.RawCode)}
                <p>This code expires in {hours} hours.
                Any earlier code is no longer valid.</p>
                """
        }, ct);
    }
}