namespace EventFlow.Application.Common.Interfaces;

public interface ICurrentTenant
{
    Guid? TenantId { get; }
}

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}