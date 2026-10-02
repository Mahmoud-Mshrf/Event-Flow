using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.CheckIn.Dtos;
using Microsoft.AspNetCore.SignalR;

namespace EventFlow.Infrastructure.Hubs;

public sealed class EventDashboardNotifier(IHubContext<EventDashboardHub,IDashboardClient> hub) : IEventDashboardNotifier
{
    public async Task NotifyDashboardUpdatedAsync(EventDashboardUpdate update)
    {
        await hub.Clients.Group(EventDashboardGroups.For(update.EventId)).UpdateStats(update);
    }
}
internal static class EventDashboardGroups
{
    public static string For(Guid eventId)
        => $"event-{eventId}";
}