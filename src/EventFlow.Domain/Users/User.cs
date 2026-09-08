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
}
