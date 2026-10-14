using EventFlow.Application.Common.Contracts;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Orders.Dtos;
using EventFlow.Application.Features.Orders.Mappers;
using EventFlow.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Orders.Queries.GetMyOrders;

public sealed class GetMyOrdersQueryHandler(
    IAppDbContext db,
    ICurrentUser currentUser)
    : IRequestHandler<GetMyOrdersQuery, Result<PaginatedList<OrderSummaryDto>>>
{
    public async Task<Result<PaginatedList<OrderSummaryDto>>> Handle(
        GetMyOrdersQuery request,
        CancellationToken ct)
    {
        var x = "not authenticated user";
        System.Console.WriteLine($"Attende Id Is : {currentUser.UserId.ToString()?? x}");
        // Load orders with their events for the event name
        var totalCount = await db.Orders
            .IgnoreQueryFilters()       // attendee has no TenantId
            .Where(o => o.AttendeeId == currentUser.UserId)
            .CountAsync(ct);

        var orders = await db.Orders
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Include(o => o.Event)
            .Where(o => o.AttendeeId == currentUser.UserId)
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