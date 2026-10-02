using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EventFlow.Infrastructure.Hubs;

[Authorize]
public sealed class EventDashboardHub : Hub
{
    
}