using EventFlow.Application.Features.CheckIn.Dtos;

namespace EventFlow.Infrastructure.Hubs;

public interface IDashboardClient
{
    Task UpdateStats(EventDashboardUpdate update);
}