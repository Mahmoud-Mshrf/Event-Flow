using EventFlow.Domain.Events;
using EventFlow.Domain.Orders;
using EventFlow.Domain.Tenants;
using EventFlow.Domain.Tickets;
using EventFlow.Domain.Users.Enums;

namespace EventFlow.Domain.Users;

public class User
{
    public Guid Id { get; private set; }

    public string PhoneNumber { get; private set; } = null!;

    public Guid? TenantId { get; private set; }

    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;

    public UserRole? Role { get; private set; }

    public bool IsActive { get; private set; }

    // Navigation properties
    public Tenant? Tenant { get; private set; }

    public ICollection<Ticket> Tickets { get; private set; }
        = new List<Ticket>();

    public ICollection<Order> Orders { get; private set; }
        = new List<Order>();
}
