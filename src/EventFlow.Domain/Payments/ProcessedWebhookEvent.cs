using EventFlow.Domain.Common;

namespace EventFlow.Domain.Payments;
// This entity's only job is to record "we've seen this webhook event before"
// It lives in Domain because it has business significance (idempotency is a business rule)
public sealed class ProcessedWebhookEvent : AuditableEntity
{
    // The payment provider's event ID — e.g. "evt_1234567890" from Stripe
    // This is what we use to detect duplicates
    public string ProviderEventId { get; private set; } = null!;

    // What kind of event — e.g. "payment_intent.succeeded"
    // Stored for debugging — "why was this event processed?"
    public string EventType { get; private set; } = null!;

    private ProcessedWebhookEvent() { } // EF Core needs this

    private ProcessedWebhookEvent(Guid id, string providerEventId, string eventType)
        : base(id)
    {
        ProviderEventId = providerEventId;
        EventType = eventType;
    }

    // No Result<T> here — this factory cannot fail for any business reason
    // Either the data is valid or it's a programmer error
    public static ProcessedWebhookEvent Create(string providerEventId, string eventType) =>
        new(Guid.NewGuid(), providerEventId, eventType);
}