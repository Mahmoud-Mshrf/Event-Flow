using System.Security.Claims;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Infrastructure.Hubs;

[Authorize]
public sealed class EventDashboardHub(IAppDbContext db) : Hub<IDashboardClient>
{
    public async Task JoinEventDashboard(string eventId)
{
    // Verify this event belongs to the caller's tenant
    // before adding them to the group
    if (!Guid.TryParse(eventId, out var eventGuid))
        return;

    var tenantIdClaim = Context.User?
        .FindFirstValue("tenant_id");

    if (!Guid.TryParse(tenantIdClaim, out var tenantId))
        return;  // no tenant context — attendee, not staff

    // Check event ownership using IAppDbContext
    // (inject via constructor — Hub supports DI)
    var eventBelongsToTenant = await db.Events
        .AnyAsync(e => e.Id == eventGuid && e.TenantId == tenantId);

    if (!eventBelongsToTenant)
        return;  // silently reject — don't reveal the event exists

    await Groups.AddToGroupAsync(
        Context.ConnectionId,
        $"event-{eventId}");
}
}