using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Events.Dtos;
using EventFlow.Application.Features.Events.Mappers;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Events.Commands.UpdateEventVisibility;

public sealed class UpdateEventVisibilityCommandHandler(
    IAppDbContext db,
    ICurrentTenant currentTenant)
    : IRequestHandler<UpdateEventVisibilityCommand, Result<EventDto>>
{
    public async Task<Result<EventDto>> Handle(
        UpdateEventVisibilityCommand request,
        CancellationToken ct)
    {
        if (currentTenant.TenantId is not { } tenantId)
            return EventErrors.InvalidTenant;

        var @event = await db.Events
            .FirstOrDefaultAsync(e => e.Id == request.EventId
                && e.TenantId == tenantId, ct);

        if (@event is null)
            return EventErrors.NotFound;

        var result = @event.UpdateVisibility(request.Visibility);

        if (result.IsError)
            return result.TopError;

        await db.SaveChangesAsync(ct);

        return @event.ToDto();
    }
}