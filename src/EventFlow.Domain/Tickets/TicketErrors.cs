using EventFlow.Domain.Common.Errors;

namespace EventFlow.Domain.Tickets;

public static class TicketErrors
{
    public static readonly Error EventIdRequired =
        Error.Validation(
            "Ticket.EventIdRequired",
            "Event ID is required.");

    public static readonly Error TenantIdRequired =
        Error.Validation(
            "Ticket.TenantIdRequired",
            "Tenant ID is required.");

    public static readonly Error AttendeeIdRequired =
        Error.Validation(
            "Ticket.AttendeeIdRequired",
            "Attendee ID is required.");

    public static readonly Error TicketTypeIdRequired =
        Error.Validation(
            "Ticket.TicketTypeIdRequired",
            "Ticket type ID is required.");

    public static readonly Error OrderIdRequired =
        Error.Validation(
            "Ticket.OrderIdRequired",
            "Order ID is required.");

    public static readonly Error InvalidTicketNumber =
        Error.Validation(
            "Ticket.InvalidTicketNumber",
            "Ticket number is required.");

    public static readonly Error QrCodeRequired =
        Error.Validation(
            "Ticket.QrCodeRequired",
            "QR code is required.");

    public static readonly Error CannotCheckIn =
        Error.Conflict(
            "Ticket.CannotCheckIn",
            "Ticket cannot be checked in.");

    public static readonly Error AlreadyCheckedIn =
        Error.Conflict(
            "Ticket.AlreadyCheckedIn",
            "Ticket has already been checked in.");

    public static readonly Error TicketCancelled =
        Error.Conflict(
            "Ticket.TicketCancelled",
            "Cancelled tickets cannot be checked in.");

    public static readonly Error CannotCancel =
        Error.Conflict(
            "Ticket.CannotCancel",
            "Ticket cannot be cancelled.");

    public static readonly Error Unauthenticated =
        Error.Unauthorized("Ticket.Unauthenticated",
            "You must be logged in to view tickets.");

    public static readonly Error NotFound =
        Error.NotFound("Ticket.NotFound",
            "The requested ticket was not found.");
    
    public static readonly Error InvalidQrCode =
        Error.Validation("Ticket.InvalidQrCode",
            "The QR code is invalid or has been tampered with.");

    public static readonly Error WrongEvent =
        Error.Conflict("Ticket.WrongEvent",
            "This ticket does not belong to this event.");
}

