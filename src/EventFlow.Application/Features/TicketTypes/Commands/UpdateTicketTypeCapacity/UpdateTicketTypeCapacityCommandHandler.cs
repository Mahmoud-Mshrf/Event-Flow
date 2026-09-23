using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.TicketTypes.Dtos;
using EventFlow.Application.Features.TicketTypes.Mappers;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.TicketTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace EventFlow.Application.Features.TicketTypes.Commands.UpdateTicketTypeCapacity;

public sealed class UpdateTicketTypeCapacityCommandHandler(
    IAppDbContext db,
    ICurrentTenant currentTenant,
    IDateTimeProvider dateTimeProvider,
    HybridCache cache)
    : IRequestHandler<UpdateTicketTypeCapacityCommand, Result<TicketTypeDto>>
{
    public async Task<Result<TicketTypeDto>> Handle(
        UpdateTicketTypeCapacityCommand request,
        CancellationToken ct)
    {
        if (currentTenant.TenantGuid is not { } tenantId)
            return TicketTypeErrors.TenantIdRequired;

        var ticketType = await db.TicketTypes
            .FirstOrDefaultAsync(tt => tt.Id == request.TicketTypeId, ct);

        if (ticketType is null)
            return TicketTypeErrors.NotFound;

        var result = ticketType.UpdateCapacity(request.Capacity);

        if (result.IsError)
            return result.TopError;

        await db.SaveChangesAsync(ct);

        await cache.RemoveByTagAsync($"tenant-{tenantId}-ticket-types", ct);

        return ticketType.ToDto(dateTimeProvider.UtcNow);
    }
}
