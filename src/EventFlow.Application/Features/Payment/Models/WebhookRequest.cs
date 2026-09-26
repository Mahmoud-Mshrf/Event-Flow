namespace EventFlow.Application.Features.Payment.Models;

public sealed record WebhookRequest(
    string RawBody,
    IReadOnlyDictionary<string, string> Headers,
    IReadOnlyDictionary<string, string> QueryParameters);
