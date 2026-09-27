using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Events.Dtos;
using EventFlow.Application.Features.Events.Mappers;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace EventFlow.Application.Features.Events.Commands.UpdateEventRegistrationPeriod;

public sealed class UpdateEventRegistrationPeriodCommandHandler(
    IAppDbContext db,
    ICurrentTenant currentTenant,
    IDateTimeProvider dateTimeProvider,
    HybridCache cache)
    : IRequestHandler<UpdateEventRegistrationPeriodCommand, Result<EventDto>>
{
    public async Task<Result<EventDto>> Handle(
        UpdateEventRegistrationPeriodCommand request,
        CancellationToken ct)
    {
        if (currentTenant.TenantGuid is not { } tenantId)
            return EventErrors.InvalidTenant;

        var @event = await db.Events
            .FirstOrDefaultAsync(e => e.Id == request.EventId, ct);

        if (@event is null)
            return EventErrors.NotFound;

        var result = @event.UpdateRegistrationPeriod(
            request.RegistrationStart,
            request.RegistrationEnd,
            dateTimeProvider.UtcNow);

        if (result.IsError)
            return result.TopError;

        await db.SaveChangesAsync(ct);

        // Invalidate all event queries for this tenant
        await cache.RemoveByTagAsync($"tenant-{tenantId}-events", ct);

        return @event.ToDto();
    }
}