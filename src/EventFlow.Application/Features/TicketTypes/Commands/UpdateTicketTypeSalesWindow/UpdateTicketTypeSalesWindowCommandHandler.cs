using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.TicketTypes.Dtos;
using EventFlow.Application.Features.TicketTypes.Mappers;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.TicketTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace EventFlow.Application.Features.TicketTypes.Commands.UpdateTicketTypeSalesWindow;

public sealed class UpdateTicketTypeSalesWindowCommandHandler(
    IAppDbContext db,
    ICurrentTenant currentTenant,
    IDateTimeProvider dateTimeProvider,
    HybridCache cache)
    : IRequestHandler<UpdateTicketTypeSalesWindowCommand, Result<TicketTypeDto>>
{
    public async Task<Result<TicketTypeDto>> Handle(
        UpdateTicketTypeSalesWindowCommand request,
        CancellationToken ct)
    {
        if (currentTenant.TenantGuid is not { } tenantId)
            return TicketTypeErrors.TenantIdRequired;

        var ticketType = await db.TicketTypes
            .FirstOrDefaultAsync(tt => tt.Id == request.TicketTypeId
                && tt.TenantId == tenantId, ct);

        if (ticketType is null)
            return TicketTypeErrors.NotFound;

        var result = ticketType.UpdateSalesWindow(
            request.SalesStart,
            request.SalesEnd,
            dateTimeProvider.UtcNow);

        if (result.IsError)
            return result.TopError;

        await db.SaveChangesAsync(ct);

        await cache.RemoveByTagAsync($"tenant-{tenantId}-ticket-types", ct);

        return ticketType.ToDto(dateTimeProvider.UtcNow);
    }
}
