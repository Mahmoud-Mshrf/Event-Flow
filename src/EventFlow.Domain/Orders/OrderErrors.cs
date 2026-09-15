using EventFlow.Domain.Common.Errors;

namespace EventFlow.Domain.Orders;

public static class OrderErrors
{
    public static readonly Error NoItems =
        Error.Validation("Order.NoItems", "An order must contain at least one item.");

    public static readonly Error InvalidItemQuantity =
        Error.Validation("Order.InvalidItemQuantity", "Each item quantity must be greater than zero.");

    public static readonly Error CannotPayNonPendingOrder =
        Error.Validation("Order.CannotPayNonPendingOrder", "Only a pending order can be marked as paid.");

    public static readonly Error CannotExpireNonPendingOrder =
        Error.Validation("Order.CannotExpireNonPendingOrder", "Only a pending order can expire.");

    public static readonly Error CannotCancelNonPendingOrder =
        Error.Validation("Order.CannotCancelNonPendingOrder", "Only a pending order can be cancelled.");
}