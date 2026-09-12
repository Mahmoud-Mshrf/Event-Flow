using EventFlow.Domain.Common.Errors;

namespace EventFlow.Domain.TicketTypes;

public static class TicketTypeErrors
{
    public static readonly Error EventIdRequired =
        Error.Validation(
            "TicketType.EventIdRequired",
            "Event ID is required.");

    public static readonly Error TenantIdRequired =
        Error.Validation(
            "TicketType.TenantIdRequired",
            "Tenant ID is required.");

    public static readonly Error InvalidName =
        Error.Validation(
            "TicketType.InvalidName",
            "Ticket type name is required and must be between 3 and 50 characters");

    public static readonly Error InvalidPrice =
        Error.Validation(
            "TicketType.InvalidPrice",
            "Ticket type price must be greater than or equal to zero.");

    public static readonly Error InvalidCapacity =
        Error.Validation(
            "TicketType.InvalidCapacity",
            "Ticket type capacity must be greater than zero.");

    public static readonly Error InvalidQuantity =
        Error.Validation(
            "TicketType.InvalidQuantity",
            "Quantity must be greater than zero.");

    public static readonly Error InvalidSalesWindow =
        Error.Validation(
            "TicketType.InvalidSalesWindow",
            "Sales start date must be before sales end date.");

    public static readonly Error InsufficientCapacity =
        Error.Validation(
            "TicketType.InsufficientCapacity",
            "There is not enough available capacity for this ticket type.");

    public static readonly Error CapacityCannotBeReduced =
        Error.Validation(
            "TicketType.CapacityCannotBeReduced",
            "Capacity cannot be reduced below the number of reserved tickets.");

    public static readonly Error CannotModifyWhileSalesActive =
        Error.Conflict(
            "TicketType.CannotModifyWhileSalesActive",
            "Ticket type cannot be modified while ticket sales are active.");

    public static readonly Error InvalidReservation =
        Error.Validation(
            "TicketType.InvalidReservation",
            "The ticket reservation is invalid.");
}