using EventFlow.Domain.Payment.Enums;

namespace EventFlow.Domain.Payment;

public class Payment
{
    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public decimal Amount { get; private set; }

    public PaymentStatus PaymentStatus { get; private set; }

    public string? ProviderTransactionId { get; private set; }
}

