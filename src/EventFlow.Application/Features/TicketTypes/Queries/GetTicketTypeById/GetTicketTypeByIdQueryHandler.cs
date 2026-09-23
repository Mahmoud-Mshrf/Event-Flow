using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.TicketTypes.Dtos;
using EventFlow.Application.Features.TicketTypes.Mappers;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.TicketTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.TicketTypes.Queries.GetTicketTypeById;

public sealed class GetTicketTypeByIdQueryHandler(
    IAppDbContext db,
    ICurrentTenant currentTenant,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<GetTicketTypeByIdQuery, Result<TicketTypeDto>>
{
    public async Task<Result<TicketTypeDto>> Handle(
        GetTicketTypeByIdQuery request,
        CancellationToken ct)
    {
        if (currentTenant.TenantGuid is not { } tenantId)
            return TicketTypeErrors.TenantIdRequired;

        var ticketType = await db.TicketTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(tt => tt.Id == request.TicketTypeId, ct);

        if (ticketType is null)
            return TicketTypeErrors.NotFound;

        return ticketType.ToDto(dateTimeProvider.UtcNow);
    }
}