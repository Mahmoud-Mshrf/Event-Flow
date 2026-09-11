using EventFlow.Domain.Common;
using EventFlow.Domain.Events;
using EventFlow.Domain.Users;

namespace EventFlow.Domain.Tenants;

public class Tenant:AuditableEntity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }

    // Navigation properties
    private readonly List<User> _users = [];

public IReadOnlyCollection<User> Users => _users;

private readonly List<Event> _events = [];

public IReadOnlyCollection<Event> Events => _events;
}