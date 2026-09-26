namespace EventFlow.Application.Features.Payments.Models;

public sealed record WebhookRequest(
    string RawBody,
    IReadOnlyDictionary<string, string> Headers,
    IReadOnlyDictionary<string, string> QueryParameters);
