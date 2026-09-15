using System.Security.Cryptography.X509Certificates;
using EventFlow.Domain.Common;
using EventFlow.Domain.Common.Results;
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

    private Tenant(Guid Id,string name , string description):base(Id)
    {
        Name = name;
        Description = description;
    }

    public static Result<Tenant> Create(Guid Id, string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name)|| name.Length < 6 || name.Length > 100)
        {
            return UserErrors.InvalidName;   // should be TenantErrors.InvalidName
        }

        return new Tenant(Id,name, description);
    }
    public Result<Updated> UpdateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description) || description.Length < 6 || description.Length > 500)
        {
            return TenantErrors.InvalidDescription;
        }

        Description = description;
        return Result.Updated;
    }
    
}