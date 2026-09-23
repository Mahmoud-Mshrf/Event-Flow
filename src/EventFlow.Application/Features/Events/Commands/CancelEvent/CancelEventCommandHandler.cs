using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace EventFlow.Application.Features.Events.Commands.CancelEvent;

public sealed class CancelEventCommandHandler(
    IAppDbContext db,
    ICurrentTenant currentTenant,HybridCache cache)
    : IRequestHandler<CancelEventCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(
        CancelEventCommand request,
        CancellationToken ct)
    {
        if (currentTenant.TenantGuid is not { } tenantId)
            return EventErrors.InvalidTenant;

        var @event = await db.Events
            .FirstOrDefaultAsync(e => e.Id == request.EventId, ct);

        if (@event is null)
            return EventErrors.NotFound;

        var result = @event.Cancel();

        if (result.IsError)
            return result.TopError;

        // SaveChangesAsync dispatches EventCancelledDomainEvent via DbContext
        // → handler cancels all Pending orders for this event
        // → releases TicketType capacity
        // → notifies attendees
        await db.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync($"tenant-{tenantId}-events", ct);
        await cache.RemoveByTagAsync("public-events", ct);
        await cache.RemoveByTagAsync($"public-event-{request.EventId}", ct);
        return Result.Success;
    }
}