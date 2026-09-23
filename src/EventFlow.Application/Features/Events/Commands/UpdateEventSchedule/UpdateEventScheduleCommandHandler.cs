using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Events.Dtos;
using EventFlow.Application.Features.Events.Mappers;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace EventFlow.Application.Features.Events.Commands.UpdateEventSchedule;

public sealed class UpdateEventScheduleCommandHandler(
    IAppDbContext db,
    ICurrentTenant currentTenant,
    IDateTimeProvider dateTimeProvider,
    HybridCache cache)
    : IRequestHandler<UpdateEventScheduleCommand, Result<EventDto>>
{
    public async Task<Result<EventDto>> Handle(
        UpdateEventScheduleCommand request,
        CancellationToken ct)
    {
        if (currentTenant.TenantGuid is not { } tenantId)
            return EventErrors.InvalidTenant;

        var @event = await db.Events
            .FirstOrDefaultAsync(e => e.Id == request.EventId, ct);

        if (@event is null)
            return EventErrors.NotFound;

        var result = @event.UpdateSchedule(
            request.StartDate,
            request.EndDate,
            dateTimeProvider.UtcNow);

        if (result.IsError)
            return result.TopError;

        await db.SaveChangesAsync(ct);

        // Invalidate all event queries for this tenant
        await cache.RemoveByTagAsync($"tenant-{tenantId}-events", ct);

        return @event.ToDto();
    }
}