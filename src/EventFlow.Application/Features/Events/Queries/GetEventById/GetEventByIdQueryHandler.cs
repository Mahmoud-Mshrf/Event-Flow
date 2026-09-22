using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Events.Dtos;
using EventFlow.Application.Features.Events.Mappers;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Events.Queries.GetEventById;

public sealed class GetEventByIdQueryHandler(
    IAppDbContext db,
    ICurrentTenant currentTenant)
    : IRequestHandler<GetEventByIdQuery, Result<EventDto>>
{
    public async Task<Result<EventDto>> Handle(
        GetEventByIdQuery request,
        CancellationToken ct)
    {
        if (currentTenant.TenantGuid is not { } tenantId)
            return EventErrors.InvalidTenant;

        var @event = await db.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.EventId
                && e.TenantId == tenantId, ct);

        if (@event is null)
            return EventErrors.NotFound;

        return @event.ToDto();
    }
}