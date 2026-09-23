using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Orders.Dtos;
using EventFlow.Application.Features.Orders.Mappers;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Orders.Queries.GetOrderById;

public sealed class GetOrderByIdQueryHandler(
    IAppDbContext db,
    ICurrentUser currentUser)
    : IRequestHandler<GetOrderByIdQuery, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> Handle(
        GetOrderByIdQuery request,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
            return OrderErrors.Unauthenticated;

        var order = await db.Orders
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Include(o => o.OrderItems)
            .Include(o => o.Event)
            .FirstOrDefaultAsync(o =>
                o.Id == request.OrderId &&
                o.AttendeeId == currentUser.UserId, ct);

        if (order is null)
            return OrderErrors.NotFound;

        // Load ticket type names for order items
        var ticketTypeIds = order.OrderItems.Select(i => i.TicketTypeId).ToList();

        var ticketTypeNames = await db.TicketTypes
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(tt => ticketTypeIds.Contains(tt.Id))
            .Select(tt => new { tt.Id, tt.Name })
            .ToDictionaryAsync(tt => tt.Id, tt => tt.Name, ct);

        var items = order.OrderItems
            .Select(item => (
                Item: item,
                TicketTypeName: ticketTypeNames.GetValueOrDefault(item.TicketTypeId, "Unknown")))
            .ToList();

        return order.ToDto(order.Event.EventName, items);
    }
}