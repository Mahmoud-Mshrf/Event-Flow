using EventFlow.Application.Common.Contracts;
using EventFlow.Application.Common.Extensions;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.PublicDiscovery.Dtos;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Events.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.PublicDiscovery.Queries.GetPublicEvents;

public sealed class GetPublicEventsQueryHandler(
    IAppDbContext db,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<GetPublicEventsQuery, Result<PaginatedList<PublicEventDto>>>
{
    public async Task<Result<PaginatedList<PublicEventDto>>> Handle(
        GetPublicEventsQuery request,
        CancellationToken ct)
    {
        var now = dateTimeProvider.UtcNow;

        var query = db.Events
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Include(e => e.Tenant)
            .Where(e => e.Visibility == EventVisibility.Public)
            .Where(e =>
                e.EventStatus == EventStatus.Published ||
                e.EventStatus == EventStatus.RegistrationOpen ||
                e.EventStatus == EventStatus.RegistrationClosed)
            .Where(e => e.StartDate >= now);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(e =>
                e.EventName.ToLower().Contains(term) ||
                e.Location.ToLower().Contains(term) ||
                e.Tenant.Name.ToLower().Contains(term));
        }

        if (request.FromDate.HasValue)
            query = query.Where(e => e.StartDate >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(e => e.StartDate <= request.ToDate.Value);

        var result = await query
            .OrderBy(e => e.StartDate)
            .Select(e => new PublicEventDto(
                e.Id,
                e.EventName,
                e.Description,
                e.Location,
                e.Tenant.Name,
                e.StartDate,
                e.EndDate,
                e.RegistrationStart,
                e.RegistrationEnd,
                e.Visibility,
                e.EventStatus == EventStatus.RegistrationOpen))
            .ToPaginatedListAsync(request.Page, request.PageSize, ct);

        return result;
    }
}