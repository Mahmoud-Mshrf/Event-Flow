using EventFlow.Domain.Common.Errors;

namespace EventFlow.Domain.Events
{
    public static class EventErrors
    {
        public static readonly Error InvalidTenant =
            Error.Validation(
                "Event.InvalidTenant",
                "A valid tenant is required.");

        public static readonly Error InvalidName =
            Error.Validation(
                "Event.InvalidName",
                "Event name must be between 6 and 100 characters.");

        public static readonly Error InvalidLocation =
            Error.Validation(
                "Event.InvalidLocation",
                "Event location is required.");

        public static readonly Error InvalidSchedule =
            Error.Validation(
                "Event.InvalidSchedule",
                "Event start date must be in the future and before the end date.");

        public static readonly Error InvalidRegistrationPeriod =
            Error.Validation(
                "Event.InvalidRegistrationPeriod",
                "Registration start and end dates must be valid and within the event period.");

        public static readonly Error InvalidVisibility =
            Error.Validation(
                "Event.InvalidVisibility",
                "Event visibility is invalid.");

        public static readonly Error DescriptionRequiredForPublishing =
            Error.Validation(
                "Event.DescriptionRequiredForPublishing",
                "Event description is required before publishing.");

        public static readonly Error LocationRequiredForPublishing =
            Error.Validation(
                "Event.LocationRequiredForPublishing",
                "Event location is required before publishing.");

        public static readonly Error TicketTypeRequiredForPublishing =
            Error.Validation(
                "Event.TicketTypeRequiredForPublishing",
                "At least one ticket type is required before publishing.");

        public static readonly Error CannotPublish =
            Error.Conflict(
                "Event.CannotPublish",
                "The event cannot be published in its current state.");

        public static readonly Error CannotOpenRegistration =
            Error.Conflict(
                "Event.CannotOpenRegistration",
                "Registration cannot be opened for this event.");

        public static readonly Error CannotCloseRegistration =
            Error.Conflict(
                "Event.CannotCloseRegistration",
                "Registration cannot be closed for this event.");

        public static readonly Error CannotComplete =
            Error.Conflict(
                "Event.CannotComplete",
                "The event cannot be completed in its current state.");

        public static readonly Error CannotCancel =
            Error.Conflict(
                "Event.CannotCancel",
                "The event cannot be cancelled in its current state.");

        public static readonly Error InvalidStatusTransition =
            Error.Conflict(
                "Event.InvalidStatusTransition",
                "The requested event status transition is not allowed.");

        public static readonly Error CannotEdit =
            Error.Conflict(
                "Event.CannotEdit",
                "The event cannot be modified in its current state.");

        public static readonly Error RegistrationNotStarted =
            Error.Conflict(
                "Event.RegistrationNotStarted",
                "Registration has not started yet.");

        public static readonly Error RegistrationPeriodEnded =
            Error.Conflict(
                "Event.RegistrationPeriodEnded",
                "The registration period has ended.");

        public static readonly Error EventNotEnded =
            Error.Conflict(
                "Event.EventNotEnded",
                "The event has not ended yet.");
    }
}