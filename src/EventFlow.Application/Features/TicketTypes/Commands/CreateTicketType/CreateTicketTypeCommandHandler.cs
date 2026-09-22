using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.TicketTypes.Dtos;
using EventFlow.Application.Features.TicketTypes.Mappers;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Events;
using EventFlow.Domain.TicketTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace EventFlow.Application.Features.TicketTypes.commands.CreateTicketType;

public sealed class CreateTicketTypeCommandHandler(
    IAppDbContext db,
    ICurrentTenant currentTenant,
    IDateTimeProvider dateTimeProvider,
    HybridCache cache)
    : IRequestHandler<CreateTicketTypeCommand, Result<TicketTypeDto>>
{
    public async Task<Result<TicketTypeDto>> Handle(
        CreateTicketTypeCommand request,
        CancellationToken ct)
    {
        if (currentTenant.TenantGuid is not { } tenantId)
            return TicketTypeErrors.TenantIdRequired;

        // Confirm the event exists and belongs to this tenant
        var eventExists = await db.Events
            .AnyAsync(e => e.Id == request.EventId
                && e.TenantId == tenantId, ct);

        if (!eventExists)
            return EventErrors.NotFound;

        var result = TicketType.Create(
            Guid.NewGuid(),
            request.EventId,
            tenantId,
            request.Name,
            request.Price,
            request.Capacity,
            request.SalesStart,
            request.SalesEnd);

        if (result.IsError)
            return result.Errors!;

        await db.TicketTypes.AddAsync(result.Value, ct);
        await db.SaveChangesAsync(ct);

        // Invalidate event cache — TicketTypes affect Publish() availability check
        await cache.RemoveByTagAsync($"tenant-{tenantId}-events", ct);
        await cache.RemoveByTagAsync($"tenant-{tenantId}-ticket-types", ct);

        return result.Value.ToDto(dateTimeProvider.UtcNow);
    }
}