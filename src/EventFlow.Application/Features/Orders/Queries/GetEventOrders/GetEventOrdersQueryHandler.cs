using EventFlow.Application.Common.Contracts;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Orders.Dtos;
using EventFlow.Application.Features.Orders.Mappers;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Orders.Queries.GetEventsOrders;

public sealed class GetEventOrdersQueryHandler(
    IAppDbContext db,
    ICurrentTenant currentTenant)
    : IRequestHandler<GetEventOrdersQuery, Result<PaginatedList<OrderSummaryDto>>>
{
    public async Task<Result<PaginatedList<OrderSummaryDto>>> Handle(
        GetEventOrdersQuery request,
        CancellationToken ct)
    {
        if (currentTenant.TenantGuid is not { } tenantId)
            return OrderErrors.Unauthenticated;

        var query = db.Orders
            .AsNoTracking()
            .Include(o => o.Event)
            .Where(o => o.EventId == request.EventId);

        if (request.Status.HasValue)
            query = query.Where(o => o.OrderStatus == request.Status.Value);

        var totalCount = await query.CountAsync(ct);

        var orders = await query
            .OrderByDescending(o => o.CreatedAtUtc)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var items = orders
            .Select(o => o.ToSummaryDto(o.Event.EventName))
            .ToList();

        return new PaginatedList<OrderSummaryDto>
        {
            PageNumber = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
            Items = items
        };
    }
}