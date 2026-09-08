using EventFlow.Domain.Events;
using EventFlow.Domain.Users;

namespace EventFlow.Domain.Tenants;

public class Tenant
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }

    // Navigation properties
    public ICollection<User> Users { get; private set; }
        = new List<User>();

    public ICollection<Event> Events { get; private set; }
        = new List<Event>();
}