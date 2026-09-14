using EventFlow.Domain.Common.Errors;

namespace EventFlow.Domain.Orders.OrderItems;

public static class OrderItemErrors
{
    public static readonly Error TenantIdRequired =
        Error.Validation(
            "OrderItem.TenantIdRequired",
            "Tenant ID is required.");

    public static readonly Error OrderIdRequired =
        Error.Validation(
            "OrderItem.OrderIdRequired",
            "Order ID is required.");

    public static readonly Error TicketTypeIdRequired =
        Error.Validation(
            "OrderItem.TicketTypeIdRequired",
            "Ticket type ID is required.");

    public static readonly Error InvalidQuantity =
        Error.Validation(
            "OrderItem.InvalidQuantity",
            "Quantity must be greater than zero.");

    public static readonly Error InvalidUnitPrice =
        Error.Validation(
            "OrderItem.InvalidUnitPrice",
            "Unit price must be greater than or equal to zero.");
}