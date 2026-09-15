using EventFlow.Domain.Common;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Events.Enums;
using EventFlow.Domain.Orders;
using EventFlow.Domain.Tenants;
using EventFlow.Domain.Tickets;
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

    private readonly List<TicketType> _ticketTypes = [];
    public IReadOnlyCollection<TicketType> TicketTypes => _ticketTypes;

    private readonly List<Ticket> _tickets = [];
    public IReadOnlyCollection<Ticket> Tickets => _tickets;

    private readonly List<Order> _orders = [];
    public IReadOnlyCollection<Order> Orders => _orders;

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
            {
                return TenantErrors.InvalidDescription;
            }
        location = location.Trim();

        if (string.IsNullOrWhiteSpace(location))
            return EventErrors.InvalidLocation;

        if (startDate <= currentTime ||
            startDate >= endDate)
            return EventErrors.InvalidSchedule;

        if (registrationStart >= registrationEnd ||
            registrationStart > startDate ||
            registrationEnd > endDate)
            return EventErrors.InvalidRegistrationPeriod;

        if (!Enum.IsDefined(visibility))
            return EventErrors.InvalidVisibility;

        description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();

        var @event = new Event(
            Guid.NewGuid(),
            tenantId,
            eventName,
            description,
            location,
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
        if (!CanEdit())
            return EventErrors.CannotEdit;

        eventName = eventName.Trim();

        if (string.IsNullOrWhiteSpace(eventName) ||
            eventName.Length < 6 ||
            eventName.Length > 100)
            return EventErrors.InvalidName;

        EventName = eventName;

        if (description is not null &&
           (description.Length < 6 || description.Length > 500))
           {
               return TenantErrors.InvalidDescription;
           }

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

        if (startDate <= currentTime ||
            startDate >= endDate)
            return EventErrors.InvalidSchedule;

        if (RegistrationStart > startDate ||
            RegistrationEnd > endDate)
            return EventErrors.InvalidRegistrationPeriod;

        StartDate = startDate;
        EndDate = endDate;

        return Result.Updated;
    }

    public Result<Updated> UpdateRegistrationPeriod(
        DateTime registrationStart,
        DateTime registrationEnd)
    {
        if (!CanEdit())
            return EventErrors.CannotEdit;

        if (registrationStart >= registrationEnd ||
            registrationStart > StartDate ||
            registrationEnd > EndDate)
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

        if (string.IsNullOrWhiteSpace(EventName) ||
            EventName.Length < 6 ||
            EventName.Length > 100)
            return EventErrors.InvalidName;

        if (string.IsNullOrWhiteSpace(Description))
            return EventErrors.DescriptionRequiredForPublishing;

        if (string.IsNullOrWhiteSpace(Location))
            return EventErrors.LocationRequiredForPublishing;

        if (StartDate <= currentTime ||
            StartDate >= EndDate)
            return EventErrors.InvalidSchedule;

        if (RegistrationStart >= RegistrationEnd ||
            RegistrationStart > StartDate ||
            RegistrationEnd > EndDate)
            return EventErrors.InvalidRegistrationPeriod;

        if (_ticketTypes.Count == 0)
            return EventErrors.TicketTypeRequiredForPublishing;

        if (!Enum.IsDefined(Visibility))
            return EventErrors.InvalidVisibility;

        EventStatus = EventStatus.Published;

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

        return Result.Success;
    }

    private bool CanEdit()
    {
        return EventStatus is EventStatus.Draft or EventStatus.Published;
    }
}

