namespace EventFlow.Application.Common.Interfaces;

public interface ICurrentTenant
{
    string? TenantId { get; }
    Guid? TenantGuid { get; } // parsed once in the implementation
}
