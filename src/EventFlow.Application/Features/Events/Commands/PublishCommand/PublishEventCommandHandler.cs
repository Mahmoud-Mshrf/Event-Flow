using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Events.Dtos;
using EventFlow.Application.Features.Events.Mappers;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Events.Commands.PublishCommand;

public sealed class PublishEventCommandHandler(
    IAppDbContext db,
    ICurrentTenant currentTenant,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<PublishEventCommand, Result<EventDto>>
{
    public async Task<Result<EventDto>> Handle(
        PublishEventCommand request,
        CancellationToken ct)
    {
        if (currentTenant.TenantId is not { } tenantId)
            return EventErrors.InvalidTenant;

        // Must include TicketTypes — Publish() checks _ticketTypes.Count internally
        var @event = await db.Events
            .Include(e => e.TicketTypes)
            .FirstOrDefaultAsync(e => e.Id == request.EventId
                && e.TenantId == tenantId, ct);

        if (@event is null)
            return EventErrors.NotFound;

        var result = @event.Publish(dateTimeProvider.UtcNow);

        if (result.IsError)
            return result.TopError;

        await db.SaveChangesAsync(ct);

        return @event.ToDto();
    }
}