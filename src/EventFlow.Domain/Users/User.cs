using System.Text.RegularExpressions;
using EventFlow.Domain.Common;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Events;
using EventFlow.Domain.Orders;
using EventFlow.Domain.Tenants;
using EventFlow.Domain.Tickets;
using EventFlow.Domain.Users.Enums;

namespace EventFlow.Domain.Users;

public class User:AuditableEntity
{
    public string PhoneNumber { get; private set; } = null!;
    public Guid? TenantId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public UserRole? Role { get; private set; }
    public bool Disabled { get; private set; }
    // Navigation properties
    public Tenant? Tenant { get; private set; }
    private readonly List<Ticket> _tickets = [];
    public IReadOnlyCollection<Ticket> Tickets => _tickets;
    private readonly List<Order> _orders = [];
    public IReadOnlyCollection<Order> Orders => _orders;

    private User(Guid id, string phoneNumber, string name, string email, string passwordHash, UserRole? role, Guid? tenantId)
        : base(id)
    {
        PhoneNumber = phoneNumber;
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        Disabled = false;
        TenantId = tenantId;
    }

    public static Result<User> CreateEmployee(Guid id, string phoneNumber, string name, string email, string passwordHash, UserRole? role, bool disabled, Guid? tenantId)
    {
        // Validate the input parameters
        if (string.IsNullOrWhiteSpace(phoneNumber) || !Regex.IsMatch(phoneNumber, @"^\+?\d{7,15}$"))
        {
            return UserErrors.InvalidPhoneNumber;
        }
        if (string.IsNullOrWhiteSpace(name))
            return UserErrors.NameIsRequired;
        if(name.Length < 6 || name.Length > 100)
            return UserErrors.InvalidName;
        if (string.IsNullOrWhiteSpace(email))
            return UserErrors.EmailIsRequired;
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            if (addr.Address != email)
                return UserErrors.InvalidEmail;
        }
        catch
        {
            return UserErrors.InvalidEmail;
        }
        if (string.IsNullOrWhiteSpace(passwordHash))
            return UserErrors.PasswordIsRequired;
        if (passwordHash.Length < 8 || !Regex.IsMatch(passwordHash, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$"))
        {
            return UserErrors.InvalidPassword;
        }
        if(role != null && !Enum.IsDefined(typeof(UserRole), role))
        {
            return UserErrors.InvalidRole;
        }
        // Create a new User instance
        var user = new User(id, phoneNumber, name, email, passwordHash, role,tenantId);

        // Raise a domain event for user creation
        // user.AddDomainEvent(new UserCreatedEvent(user.Id));

        return user;
    }
    public Result<User> CreateAttendee(Guid id, string phoneNumber, string name, string email, string passwordHash)
    {
        // Validate the input parameters
        if (string.IsNullOrWhiteSpace(phoneNumber) || !Regex.IsMatch(phoneNumber, @"^\+?\d{7,15}$"))
        {
            return UserErrors.InvalidPhoneNumber;
        }
        if (string.IsNullOrWhiteSpace(name))
            return UserErrors.NameIsRequired;
        if(name.Length < 6 || name.Length > 100)
            return UserErrors.InvalidName;
        if (string.IsNullOrWhiteSpace(email))
            return UserErrors.EmailIsRequired;
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            if (addr.Address != email)
                return UserErrors.InvalidEmail;
        }
        catch
        {
            return UserErrors.InvalidEmail;
        }
        if (string.IsNullOrWhiteSpace(passwordHash))
            return UserErrors.PasswordIsRequired;
        if (passwordHash.Length < 8 || !Regex.IsMatch(passwordHash, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$"))
        {
            return UserErrors.InvalidPassword;
        }

        // Create a new User instance
        var user = new User(id, phoneNumber, name, email, passwordHash, null,null);

        // Raise a domain event for user creation
        // user.AddDomainEvent(new UserCreatedEvent(user.Id));

        return user;
    }
}