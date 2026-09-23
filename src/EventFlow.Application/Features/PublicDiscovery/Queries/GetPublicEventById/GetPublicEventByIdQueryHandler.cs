using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.PublicDiscovery.Dtos;
using EventFlow.Application.Features.PublicDiscovery.Mappers;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Events;
using EventFlow.Domain.Events.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.PublicDiscovery.Queries.GetPublicEventById;

public sealed class GetPublicEventByIdQueryHandler(
    IAppDbContext db,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<GetPublicEventByIdQuery, Result<PublicEventDetailDto>>
{
    public async Task<Result<PublicEventDetailDto>> Handle(
        GetPublicEventByIdQuery request,
        CancellationToken ct)
    {
        var now = dateTimeProvider.UtcNow;

        var @event = await db.Events
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Include(e => e.Tenant)
            .Include(e => e.TicketTypes)
            .Where(e =>
                e.Id == request.EventId &&
                e.Visibility == EventVisibility.Public &&
                (e.EventStatus == EventStatus.Published ||
                 e.EventStatus == EventStatus.RegistrationOpen ||
                 e.EventStatus == EventStatus.RegistrationClosed))
            .FirstOrDefaultAsync(ct);

        if (@event is null)
            return EventErrors.NotFound;

        return @event.ToDetailDto(now);
    }
}