using EventFlow.Application.Common.Contracts;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Tickets.Dtos;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Tickets;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Tickets.Queries.GetMyTickets;

public sealed class GetMyTicketsQueryHandler(
    IAppDbContext db,
    ICurrentUser currentUser)
    : IRequestHandler<GetMyTicketsQuery, Result<PaginatedList<TicketSummaryDto>>>
{
    public async Task<Result<PaginatedList<TicketSummaryDto>>> Handle(
        GetMyTicketsQuery request,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
            return TicketErrors.Unauthenticated;

        var query = db.Tickets
            .AsNoTracking()
            .IgnoreQueryFilters()           // attendee has no TenantId
            .Include(t => t.Event)
            .Where(t => t.AttendeeId == currentUser.UserId);

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