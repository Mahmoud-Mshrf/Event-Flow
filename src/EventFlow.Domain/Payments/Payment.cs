using EventFlow.Domain.Common;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Orders;
using EventFlow.Domain.Payments.Enums;
using EventFlow.Domain.Payments.Events;

namespace EventFlow.Domain.Payments;

public class Payment : AuditableEntity
{
    public Guid TenantId { get; private set; }
    public Guid OrderId { get; private set; }
    public Order Order {get;private set;} =null!;
    public decimal Amount { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public string ProviderReferenceId { get; private set; } = null!;
    public DateTime? SucceededAt { get; private set; }
    public DateTime? FailedAt { get; private set; }
    public string? FailureReason { get; private set; }

    private Payment() { } // EF Core

    private Payment(Guid id, Guid tenantId, Guid orderId, decimal amount, string providerReferenceId)
        : base(id)
    {
        TenantId = tenantId;
        OrderId = orderId;
        Amount = amount;
        ProviderReferenceId = providerReferenceId;
        PaymentStatus = PaymentStatus.Pending;
    }

    public static Result<Payment> Initiate(
        Guid id, Guid tenantId, Guid orderId, decimal amount, string providerReferenceId)
    {
        if (tenantId == Guid.Empty)
            return PaymentErrors.TenantIdRequired;

        if (orderId == Guid.Empty)
            return PaymentErrors.OrderIdRequired;

        if (amount <= 0)
            return PaymentErrors.InvalidAmount;

        if (string.IsNullOrWhiteSpace(providerReferenceId))
            return PaymentErrors.MissingProviderReference;

        return new Payment(id, tenantId, orderId, amount, providerReferenceId);
    }

    public Result<Success> MarkSucceeded(DateTime succeededAtUtc)
    {
        if (PaymentStatus != PaymentStatus.Pending)
            return PaymentErrors.CannotTransitionFromNonPending;

        PaymentStatus = PaymentStatus.Succeeded;
        SucceededAt = succeededAtUtc;

        AddDomainEvent(new PaymentSucceededDomainEvent(Id, OrderId, TenantId));

        return Result.Success;
    }

    public Result<Success> MarkFailed(string reason, DateTime failedAtUtc)
    {
        if (PaymentStatus != PaymentStatus.Pending)
            return PaymentErrors.CannotTransitionFromNonPending;

        PaymentStatus = PaymentStatus.Failed;
        FailedAt = failedAtUtc;
        FailureReason = reason;

        return Result.Success;
    }
}

