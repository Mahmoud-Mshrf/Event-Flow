using EventFlow.Application.Common.Interfaces;

namespace EventFlow.Infrastructure.Services;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}