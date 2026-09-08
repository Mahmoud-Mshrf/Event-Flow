using EventFlow.Domain.Events.Enums;
using EventFlow.Domain.Orders;
using EventFlow.Domain.Tenants;
using EventFlow.Domain.Tickets;
using EventFlow.Domain.TicketTypes;

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

    // Navigation properties
    public Tenant Tenant { get; private set; } = null!;

    public ICollection<TicketType> TicketTypes { get; private set; }
        = new List<TicketType>();

    public ICollection<Ticket> Tickets { get; private set; }
        = new List<Ticket>();

    public ICollection<Order> Orders { get; private set; }
        = new List<Order>();
}