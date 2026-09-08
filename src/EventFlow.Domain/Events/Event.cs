using EventFlow.Domain.Events.Enums;

namespace EventFlow.Domain.Events;

public class Event
{
    public Guid Id { get; private set; }

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
}