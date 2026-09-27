using EventFlow.Domain.Common;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Events.DomainEvents;
using EventFlow.Domain.Events.Enums;
using EventFlow.Domain.Tenants;
using EventFlow.Domain.TicketTypes;

namespace EventFlow.Domain.Events;

public class Event : AuditableEntity
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

    // TicketTypes stay — Event.Publish() legitimately needs _ticketTypes.Count
    // and TicketType lifecycle is tied to the Event (can't exist without it)
    private readonly List<TicketType> _ticketTypes = [];
    public IReadOnlyCollection<TicketType> TicketTypes => _ticketTypes;

    // Tickets and Orders removed — they are separate aggregate roots
    // Access them via db.Tickets.Where(t => t.EventId == id)
    // and db.Orders.Where(o => o.EventId == id) in read-side queries

    private Event() { } // EF Core

    private Event(
        Guid id,
        Guid tenantId,
        string eventName,
        string? description,
        string location,
        DateTime startDate,
        DateTime endDate,
        DateTime registrationStart,
        DateTime registrationEnd,
        EventVisibility visibility)
        : base(id)
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

    public static Result<Event> Create(
        Guid id,                    // fix #2 — caller supplies id, consistent with other entities
        Guid tenantId,
        string eventName,
        string? description,
        string location,
        DateTime startDate,
        DateTime endDate,
        DateTime registrationStart,
        DateTime registrationEnd,
        EventVisibility visibility,
        DateTime currentTime)
    {
        if (tenantId == Guid.Empty)
            return EventErrors.InvalidTenant;

        eventName = eventName.Trim();

        if (string.IsNullOrWhiteSpace(eventName) ||
            eventName.Length < 6 ||
            eventName.Length > 100)
            return EventErrors.InvalidName;

        if (description is not null &&
           (description.Length < 6 || description.Length > 500))
            return EventErrors.InvalidDescription;  // fix #4 — was TenantErrors

        location = location.Trim();

        if (string.IsNullOrWhiteSpace(location))
            return EventErrors.InvalidLocation;

        if (startDate <= currentTime || startDate >= endDate)
            return EventErrors.InvalidSchedule;

        if (registrationStart <= currentTime ||
            registrationEnd <= currentTime ||
            registrationStart >= registrationEnd ||
            registrationEnd >= startDate)
            return EventErrors.InvalidRegistrationPeriod;

        if (!Enum.IsDefined(visibility))
            return EventErrors.InvalidVisibility;

        description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();

        return new Event(
            id,
            tenantId,
            eventName,
            description,
            location,
            startDate,
            endDate,
            registrationStart,
            registrationEnd,
            visibility);
    }

    public Result<Updated> UpdateDetails(
        string eventName,
        string? description)
    {
        if (!CanEdit())
            return EventErrors.CannotEdit;

        eventName = eventName.Trim();

        if (string.IsNullOrWhiteSpace(eventName) ||
            eventName.Length < 6 ||
            eventName.Length > 100)
            return EventErrors.InvalidName;

        if (description is not null &&
           (description.Length < 6 || description.Length > 500))
            return EventErrors.InvalidDescription;  // fix #4

        EventName = eventName;
        Description = string.IsNullOrWhiteSpace(description)  // fix #3 — was never set
            ? null
            : description.Trim();

        return Result.Updated;
    }

    public Result<Updated> UpdateLocation(string location)
    {
        if (!CanEdit())
            return EventErrors.CannotEdit;

        location = location.Trim();

        if (string.IsNullOrWhiteSpace(location))
            return EventErrors.InvalidLocation;

        Location = location;

        return Result.Updated;
    }

    public Result<Updated> UpdateSchedule(
        DateTime startDate,
        DateTime endDate,
        DateTime currentTime)
    {
        if (!CanEdit())
            return EventErrors.CannotEdit;

        if (startDate <= currentTime || startDate >= endDate)
            return EventErrors.InvalidSchedule;

        if (RegistrationEnd >= startDate)
            return EventErrors.InvalidRegistrationPeriod;

        StartDate = startDate;
        EndDate = endDate;

        return Result.Updated;
    }

    public Result<Updated> UpdateRegistrationPeriod(
        DateTime registrationStart,
        DateTime registrationEnd,
        DateTime currentTime)
    {
        if (!CanEdit())
            return EventErrors.CannotEdit;

        if (registrationStart <= currentTime ||
            registrationEnd <= currentTime ||
            registrationStart >= registrationEnd ||
            registrationEnd >= StartDate)
            return EventErrors.InvalidRegistrationPeriod;

        RegistrationStart = registrationStart;
        RegistrationEnd = registrationEnd;

        return Result.Updated;
    }

    public Result<Updated> UpdateVisibility(EventVisibility visibility)
    {
        if (!CanEdit())
            return EventErrors.CannotEdit;

        if (!Enum.IsDefined(visibility))
            return EventErrors.InvalidVisibility;

        Visibility = visibility;

        return Result.Updated;
    }

    public Result<Success> Publish(DateTime currentTime)
    {
        if (EventStatus != EventStatus.Draft)
            return EventErrors.CannotPublish;

        if (string.IsNullOrWhiteSpace(Description))
            return EventErrors.DescriptionRequiredForPublishing;

        if (string.IsNullOrWhiteSpace(Location))
            return EventErrors.LocationRequiredForPublishing;

        if (StartDate <= currentTime || StartDate >= EndDate)
            return EventErrors.InvalidSchedule;

        if (RegistrationStart >= RegistrationEnd ||
            RegistrationEnd >= StartDate)
            return EventErrors.InvalidRegistrationPeriod;

        if (_ticketTypes.Count == 0)
            return EventErrors.TicketTypeRequiredForPublishing;

        EventStatus = EventStatus.Published;

        AddDomainEvent(new EventPublishedDomainEvent(Id, TenantId, Visibility));

        return Result.Success;
    }

    public Result<Success> OpenRegistration(DateTime currentTime)
    {
        if (EventStatus != EventStatus.Published)
            return EventErrors.CannotOpenRegistration;

        if (currentTime < RegistrationStart)
            return EventErrors.RegistrationNotStarted;

        if (currentTime >= RegistrationEnd)
            return EventErrors.RegistrationPeriodEnded;

        EventStatus = EventStatus.RegistrationOpen;

        return Result.Success;
    }

    public Result<Success> CloseRegistration()
    {
        if (EventStatus != EventStatus.RegistrationOpen)
            return EventErrors.CannotCloseRegistration;

        EventStatus = EventStatus.RegistrationClosed;

        return Result.Success;
    }

    public Result<Success> Complete(DateTime currentTime)
    {
        if (EventStatus != EventStatus.RegistrationClosed)
            return EventErrors.CannotComplete;

        if (currentTime < EndDate)
            return EventErrors.EventNotEnded;

        EventStatus = EventStatus.Completed;

        return Result.Success;
    }

    public Result<Success> Cancel()
    {
        if (EventStatus is EventStatus.Completed or EventStatus.Cancelled)
            return EventErrors.CannotCancel;

        EventStatus = EventStatus.Cancelled;

        AddDomainEvent(new EventCancelledDomainEvent(Id, TenantId));

        return Result.Success;
    }

    private bool CanEdit() =>
        EventStatus is EventStatus.Draft or EventStatus.Published;
}

