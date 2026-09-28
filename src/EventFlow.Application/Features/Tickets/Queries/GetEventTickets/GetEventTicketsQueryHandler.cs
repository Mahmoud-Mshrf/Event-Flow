using EventFlow.Application.Common.Contracts;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Tickets.Dtos;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Tickets;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Tickets.Queries.GetEventTickets;

public sealed class GetEventTicketsQueryHandler(
    IAppDbContext db,
    ICurrentTenant currentTenant)
    : IRequestHandler<GetEventTicketsQuery, Result<PaginatedList<TicketSummaryDto>>>
{
    public async Task<Result<PaginatedList<TicketSummaryDto>>> Handle(
        GetEventTicketsQuery request,
        CancellationToken ct)
    {
        if (currentTenant.TenantGuid is not { } tenantId)
            return TicketErrors.Unauthenticated;

        var query = db.Tickets
            .AsNoTracking()
            .Include(t => t.Event)
            .Where(t => t.EventId == request.EventId);
            // Global filter already scopes to TenantId
            // No .Where(t => t.TenantId == tenantId) needed

        if (request.Status.HasValue)
            query = query.Where(t => t.Status == request.Status.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(t => t.CreatedAtUtc)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var dtos = items
            .Select(t => new TicketSummaryDto(
                t.Id,
                t.TicketNumber,
                t.Event.EventName,
                t.Status))
            .ToList();

        return new PaginatedList<TicketSummaryDto>
        {
            PageNumber = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
            Items = dtos
        };
    }
}