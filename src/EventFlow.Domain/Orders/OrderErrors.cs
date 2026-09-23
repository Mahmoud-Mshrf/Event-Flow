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
    public static readonly Error Unauthenticated =
        Error.Unauthorized("Order.Unauthenticated",
            "You must be logged in to place an order.");

    public static readonly Error RegistrationNotOpen =
        Error.Conflict("Order.RegistrationNotOpen",
            "This event is not currently accepting registrations.");

    public static readonly Error InvalidTicketTypes =
        Error.Validation("Order.InvalidTicketTypes",
            "One or more ticket types do not belong to this event.");

    public static Error TicketSalesNotOpen(string ticketTypeName) =>
        Error.Conflict("Order.TicketSalesNotOpen",
            $"Sales for '{ticketTypeName}' are not currently open.");

    public static readonly Error ConcurrencyConflict =
        Error.Conflict("Order.ConcurrencyConflict",
            "The tickets you requested are no longer available. Please try again.");

    public static readonly Error NotFound =
        Error.NotFound("Order.NotFound",
            "The requested order was not found.");

    public static readonly Error CannotCancel =
        Error.Conflict("Order.CannotCancel",
            "Only pending orders can be cancelled.");

    public static readonly Error Unauthorized =
        Error.Forbidden("Order.Unauthorized",
            "You are not authorized to access this order.");    
}