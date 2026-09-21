using EventFlow.Application.Common.Contracts;
using EventFlow.Application.Common.Extensions;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Events.Dtos;
using EventFlow.Application.Features.Events.Mappers;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Events.Queries.GetOrganizerEvents;

public sealed class GetOrganizerEventsQueryHandler(
    IAppDbContext db,
    ICurrentTenant currentTenant)
    : IRequestHandler<GetOrganizerEventsQuery, Result<PaginatedList<EventDto>>>
{
    public async Task<Result<PaginatedList<EventDto>>> Handle(
        GetOrganizerEventsQuery request,
        CancellationToken ct)
    {
        if (currentTenant.TenantId is not { } tenantId)
            return EventErrors.InvalidTenant;

        var result = await db.Events
            .AsNoTracking()
            .Where(e => e.TenantId == tenantId)
            .Where(e => !request.Status.HasValue || e.EventStatus == request.Status.Value)
            .Where(e => !request.Visibility.HasValue || e.Visibility == request.Visibility.Value)
            .OrderByDescending(e => e.StartDate)
            .Select(e => e.ToDto())
            .ToPaginatedListAsync(request.Page, request.PageSize, ct);

        return result;
    }
}
