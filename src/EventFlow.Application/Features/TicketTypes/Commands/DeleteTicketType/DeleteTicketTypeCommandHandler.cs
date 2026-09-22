using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.TicketTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace EventFlow.Application.Features.TicketTypes.Commands.DeleteTicketType;

public sealed class DeleteTicketTypeCommandHandler(
    IAppDbContext db,
    ICurrentTenant currentTenant,
    HybridCache cache)
    : IRequestHandler<DeleteTicketTypeCommand, Result<Deleted>>
{
    public async Task<Result<Deleted>> Handle(
        DeleteTicketTypeCommand request,
        CancellationToken ct)
    {
        if (currentTenant.TenantGuid is not { } tenantId)
            return TicketTypeErrors.TenantIdRequired;

        var ticketType = await db.TicketTypes
            .FirstOrDefaultAsync(tt => tt.Id == request.TicketTypeId
                && tt.TenantId == tenantId, ct);

        if (ticketType is null)
            return TicketTypeErrors.NotFound;

        // Cannot delete a ticket type that has active reservations
        if (ticketType.ReservedQuantity > 0)
            return TicketTypeErrors.CannotDeleteWithActiveReservations;

        db.TicketTypes.Remove(ticketType);
        await db.SaveChangesAsync(ct);

        await cache.RemoveByTagAsync($"tenant-{tenantId}-ticket-types", ct);
        await cache.RemoveByTagAsync($"tenant-{tenantId}-events", ct);

        return Result.Deleted;
    }
}
