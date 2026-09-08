namespace EventFlow.Domain.Tenants;

public class Tenant
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
}