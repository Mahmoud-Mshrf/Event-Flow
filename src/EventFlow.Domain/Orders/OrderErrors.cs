using EventFlow.Domain.Common.Errors;

namespace EventFlow.Domain.Orders;

public static class OrderErrors
{
    public static readonly Error EventIdRequired =
        Error.Validation(
            "Order.EventIdRequired",
            "Event ID is required.");

    public static readonly Error TenantIdRequired =
        Error.Validation(
            "Order.TenantIdRequired",
            "Tenant ID is required.");

    public static readonly Error AttendeeIdRequired =
        Error.Validation(
            "Order.AttendeeIdRequired",
            "Attendee ID is required.");

    public static readonly Error InvalidOrderNumber =
        Error.Validation(
            "Order.InvalidOrderNumber",
            "Order number is required.");

    public static readonly Error InvalidQuantity =
        Error.Validation(
            "Order.InvalidQuantity",
            "Quantity must be greater than zero.");

    public static readonly Error InvalidTotal =
        Error.Validation(
            "Order.InvalidTotal",
            "Order total cannot be negative.");

    public static readonly Error CannotAddItem =
        Error.Conflict(
            "Order.CannotAddItem",
            "Items cannot be added to the order in its current state.");

    public static readonly Error CannotMarkAsPaid =
        Error.Conflict(
            "Order.CannotMarkAsPaid",
            "Order cannot be marked as paid in its current state.");

    public static readonly Error CannotExpire =
        Error.Conflict(
            "Order.CannotExpire",
            "Order cannot be expired in its current state.");

    public static readonly Error CannotCancel =
        Error.Conflict(
            "Order.CannotCancel",
            "Order cannot be cancelled in its current state.");

    public static readonly Error OrderAlreadyPaid =
        Error.Conflict(
            "Order.OrderAlreadyPaid",
            "Order has already been paid.");
}