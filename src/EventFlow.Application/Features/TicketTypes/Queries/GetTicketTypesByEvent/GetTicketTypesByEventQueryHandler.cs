using EventFlow.Application.Common.Contracts;
using EventFlow.Application.Common.Extensions;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.TicketTypes.Dtos;
using EventFlow.Application.Features.TicketTypes.Mappers;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.TicketTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.TicketTypes.Queries.GetTicketTypesByEvent;

public sealed class GetTicketTypesByEventQueryHandler(
    IAppDbContext db,
    ICurrentTenant currentTenant,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<GetTicketTypesByEventQuery, Result<PaginatedList<TicketTypeDto>>>
{
    public async Task<Result<PaginatedList<TicketTypeDto>>> Handle(
        GetTicketTypesByEventQuery request,
        CancellationToken ct)
    {
        if (currentTenant.TenantGuid is not { } tenantId)
            return TicketTypeErrors.TenantIdRequired;

        var now = dateTimeProvider.UtcNow;

        var result = await db.TicketTypes
            .AsNoTracking()
            .Where(tt => tt.EventId == request.EventId
                && tt.TenantId == tenantId)
            .OrderBy(tt => tt.Price)
            .Select(tt => tt.ToDto(now))
            .ToPaginatedListAsync(request.Page, request.PageSize, ct);

        return result;
    }
}