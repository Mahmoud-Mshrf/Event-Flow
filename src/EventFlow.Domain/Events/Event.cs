using EventFlow.Domain.Common;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Events.Enums;
using EventFlow.Domain.Orders;
using EventFlow.Domain.Tenants;
using EventFlow.Domain.Tickets;
using EventFlow.Domain.TicketTypes;

namespace EventFlow.Domain.Events;

// public class Event
// {
//     public Guid Id { get; private set; }

//     public Guid TenantId { get; private set; }

//     public string EventName { get; private set; } = null!;
//     public EventStatus EventStatus { get; private set; }

//     public string? Description { get; private set; }
//     public string Location { get; private set; } = null!;

//     public DateTime StartDate { get; private set; }
//     public DateTime EndDate { get; private set; }

//     public DateTime RegistrationStart { get; private set; }
//     public DateTime RegistrationEnd { get; private set; }

//     public EventVisibility Visibility { get; private set; }

//     // Navigation properties
//     public Tenant Tenant { get; private set; } = null!;

//     private readonly List<TicketType> _ticketTypes = [];
//     public IReadOnlyCollection<TicketType> TicketTypes => _ticketTypes;

//     private readonly List<Ticket> _tickets = [];
//     public IReadOnlyCollection<Ticket> Tickets => _tickets;

//     private readonly List<Order> _orders = [];
//     public IReadOnlyCollection<Order> Orders => _orders;
// }

public class Event:AuditableEntity
{
    public Guid TenantId { get; private set; }
    public string EventName { get; private set; } = null!;
    public EventStatus EventStatus { get; private set; }
    public string? Description { get; private set; }
    public string Location { get; private set; } = null!;

    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    public DateTime RegistrationStart { get; private set; }
    public DateTime RegistrationEnd { get; private set; }

    public EventVisibility Visibility { get; private set; }

    // Navigation properties
    public Tenant Tenant { get; private set; } = null!;

    private readonly List<TicketType> _ticketTypes = [];
    public IReadOnlyCollection<TicketType> TicketTypes => _ticketTypes;

    private readonly List<Ticket> _tickets = [];
    public IReadOnlyCollection<Ticket> Tickets => _tickets;

    private readonly List<Order> _orders = [];
    public IReadOnlyCollection<Order> Orders => _orders;


    // EF Core
    private Event(Guid id,Guid tenantId,
        string eventName,
        string? description,
        string location,
        DateTime startDate,
        DateTime endDate,
        DateTime registrationStart,
        DateTime registrationEnd,
        EventVisibility visibility):base(id)
    {
        TenantId = tenantId;
        EventName = eventName;
        Description = description;
        Location = location;
        StartDate = startDate;
        EndDate = endDate;
        RegistrationStart = registrationStart;
        RegistrationEnd = registrationEnd;
        Visibility = visibility;
        EventStatus = EventStatus.Draft;
    }

    // Factory Method
    public static Result<Event> Create(
        Guid tenantId,
        string eventName,
        string? description,
        string location,
        DateTime startDate,
        DateTime endDate,
        DateTime registrationStart,
        DateTime registrationEnd,
        EventVisibility visibility)
    {
        if (tenantId == Guid.Empty)
            return EventErrors.InvalidTenant;

        if (string.IsNullOrWhiteSpace(eventName))
            return EventErrors.InvalidName;

        if (string.IsNullOrWhiteSpace(location))
            return EventErrors.InvalidLocation;

        if (startDate >= endDate)
            return EventErrors.InvalidSchedule;

        if (registrationStart >= registrationEnd)
            return EventErrors.InvalidRegistrationPeriod;

        if (visibility is not EventVisibility.Public
            and not EventVisibility.Private)
            return EventErrors.InvalidVisibility;

        var @event = new Event(
            Guid.NewGuid(),
            tenantId,
            eventName.Trim(),
            string.IsNullOrWhiteSpace(description)
                ? null
                : description.Trim(),
            location.Trim(),
            startDate,
            endDate,
            registrationStart,
            registrationEnd,
            visibility);
        return @event;
    }


    public Result<Updated> UpdateDetails(
        string eventName,
        string? description)
    {
        if (EventStatus != EventStatus.Draft &&
            EventStatus != EventStatus.Published)
            return EventErrors.CannotEdit;

        if (string.IsNullOrWhiteSpace(eventName)|| eventName.Length < 6 || eventName.Length > 100)
            return EventErrors.InvalidName;

        EventName = eventName.Trim();

        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();

        return Result.Updated;
    }


    public Result<Updated> UpdateLocation(string location)
    {
        if (EventStatus != EventStatus.Draft &&
            EventStatus != EventStatus.Published)
            return EventErrors.CannotEdit;

        if (string.IsNullOrWhiteSpace(location))
            return EventErrors.InvalidLocation;

        Location = location.Trim();

        return Result.Updated;
    }


    public Result<Updated> UpdateSchedule(
        DateTime startDate,
        DateTime endDate)
    {
        if (EventStatus != EventStatus.Draft &&
            EventStatus != EventStatus.Published)
            return EventErrors.CannotEdit;

        if (startDate >= endDate)
            return EventErrors.InvalidSchedule;

        StartDate = startDate;
        EndDate = endDate;

        return Result.Updated;
    }


    public Result<Updated> UpdateRegistrationPeriod(
        DateTime registrationStart,
        DateTime registrationEnd)
    {
        if (EventStatus != EventStatus.Draft &&
            EventStatus != EventStatus.Published)
            return EventErrors.CannotEdit;

        if (registrationStart >= registrationEnd)
            return EventErrors.InvalidRegistrationPeriod;

        RegistrationStart = registrationStart;
        RegistrationEnd = registrationEnd;

        return Result.Updated;
    }


    public Result<Updated> UpdateVisibility(EventVisibility visibility)
    {
        if (EventStatus != EventStatus.Draft &&
            EventStatus != EventStatus.Published)
            return EventErrors.CannotEdit;

        if (visibility is not EventVisibility.Public
            and not EventVisibility.Private)
            return EventErrors.InvalidVisibility;

        Visibility = visibility;

        return Result.Updated;
    }


    public Result<Updated> Publish()
    {
        if (EventStatus != EventStatus.Draft)
            return EventErrors.CannotPublish;

        if (string.IsNullOrWhiteSpace(EventName))
            return EventErrors.InvalidName;

        if (string.IsNullOrWhiteSpace(Description))
            return EventErrors.DescriptionRequiredForPublishing;

        if (string.IsNullOrWhiteSpace(Location))
            return EventErrors.LocationRequiredForPublishing;

        if (StartDate >= EndDate)
            return EventErrors.ScheduleRequiredForPublishing;

        if (RegistrationStart >= RegistrationEnd)
            return EventErrors.RegistrationPeriodRequiredForPublishing;

        if (_ticketTypes.Count == 0)
            return EventErrors.TicketTypeRequiredForPublishing;

        EventStatus = EventStatus.Published;

        return Result.Updated;
    }


    public Result<Updated> OpenRegistration()
    {
        if (EventStatus != EventStatus.Published)
            return EventErrors.CannotOpenRegistration;

        EventStatus = EventStatus.RegistrationOpen;

        return Result.Updated;
    }


    public Result<Updated> CloseRegistration()
    {
        if (EventStatus != EventStatus.RegistrationOpen)
            return EventErrors.CannotCloseRegistration;

        EventStatus = EventStatus.RegistrationClosed;

        return Result.Updated;
    }


    public Result<Updated> Complete()
    {
        if (EventStatus != EventStatus.RegistrationClosed)
            return EventErrors.CannotComplete;

        EventStatus = EventStatus.Completed;

        return Result.Updated;
    }


    public Result<Updated> Cancel()
    {
        if (EventStatus is EventStatus.Completed
            or EventStatus.Cancelled)
            return EventErrors.CannotCancel;

        EventStatus = EventStatus.Cancelled;

        return Result.Updated;
    }
}