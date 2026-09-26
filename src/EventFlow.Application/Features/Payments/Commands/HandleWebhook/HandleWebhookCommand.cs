using EventFlow.Application.Features.Payments.Models;
using EventFlow.Domain.Common.Results;
using MediatR;

namespace EventFlow.Application.Features.Payments.Commands.HandleWebhook;

// Triggered by the webhook endpoint — not by a user
public sealed record HandleWebhookCommand(
    WebhookRequest Request) : IRequest<Result<Success>>;
