using EventFlow.Domain.Common.Errors;

namespace EventFlow.Domain.Events;

public static class EventErrors
{
    // General validation

    public static readonly Error InvalidName =
        Error.Validation(
            "Event.InvalidName",
            "Event name is required.");

    public static readonly Error InvalidLocation =
        Error.Validation(
            "Event.InvalidLocation",
            "Event location is required.");

    public static readonly Error InvalidSchedule =
        Error.Validation(
            "Event.InvalidSchedule",
            "Event start date must be before the end date.");

    public static readonly Error InvalidRegistrationPeriod =
        Error.Validation(
            "Event.InvalidRegistrationPeriod",
            "Event registration start must be before registration end.");

    public static readonly Error InvalidVisibility =
        Error.Validation(
            "Event.InvalidVisibility",
            "Event visibility is invalid.");


    // Publishing requirements

    public static readonly Error DescriptionRequiredForPublishing =
        Error.Validation(
            "Event.DescriptionRequiredForPublishing",
            "Event description is required before publishing.");

    public static readonly Error LocationRequiredForPublishing =
        Error.Validation(
            "Event.LocationRequiredForPublishing",
            "Event location is required before publishing.");

    public static readonly Error ScheduleRequiredForPublishing =
        Error.Validation(
            "Event.ScheduleRequiredForPublishing",
            "Event start and end dates are required before publishing.");

    public static readonly Error RegistrationPeriodRequiredForPublishing =
        Error.Validation(
            "Event.RegistrationPeriodRequiredForPublishing",
            "Registration start and end dates are required before publishing.");

    public static readonly Error TicketTypeRequiredForPublishing =
        Error.Validation(
            "Event.TicketTypeRequiredForPublishing",
            "At least one ticket type is required before publishing.");


    // Lifecycle

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


    // Modification rules

    public static readonly Error CannotEdit =
        Error.Conflict(
            "Event.CannotEdit",
            "The event cannot be modified in its current state.");
}