using EventFlow.Application.Features.CheckIn.Dtos;

namespace EventFlow.Application.Common.Interfaces;

public interface IEventDashboardNotifier
{
    Task NotifyDashboardUpdatedAsync(EventDashboardUpdate update);
}