using EventFlow.Domain.Common;
using EventFlow.Domain.Orders;
using EventFlow.Domain.Payments.Enums;

namespace EventFlow.Domain.Payments;

public class Payment:AuditableEntity
{
    public Guid OrderId { get; private set; }

    public decimal Amount { get; private set; }

    public PaymentStatus PaymentStatus { get; private set; }

    public string? ProviderTransactionId { get; private set; }

    // Navigation properties
    public Order Order { get; private set; } = null!;
}

