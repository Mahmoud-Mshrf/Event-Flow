using System.Security.Cryptography;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Orders.Dtos;
using EventFlow.Application.Features.Orders.Mappers;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Events;
using EventFlow.Domain.Events.Enums;
using EventFlow.Domain.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler(
    IAppDbContext db,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> Handle(
        CreateOrderCommand request,
        CancellationToken ct)
    {
        // 1. Confirm the caller is an authenticated attendee
        if (!currentUser.IsAuthenticated)
            return OrderErrors.Unauthenticated;

        var attendeeId = currentUser.UserId;

        // 2. Load the event — must be open for registration
        var @event = await db.Events
            .AsNoTracking()
            .IgnoreQueryFilters()           // cross-tenant read — attendee has no TenantId
            .FirstOrDefaultAsync(e => e.Id == request.EventId, ct);

        if (@event is null)
            return EventErrors.NotFound;

        if (@event.EventStatus != EventStatus.RegistrationOpen)
            return OrderErrors.RegistrationNotOpen;

        // 3. Load the requested ticket types — WITH tracking, we're about to mutate them
        //    Use IgnoreQueryFilters — attendee has no TenantId
        var ticketTypeIds = request.Items.Select(i => i.TicketTypeId).ToList();

        var ticketTypes = await db.TicketTypes
            .IgnoreQueryFilters()
            .Where(tt => ticketTypeIds.Contains(tt.Id)
                && tt.EventId == request.EventId)
            .ToListAsync(ct);

        // 4. Validate all requested ticket types exist for this event
        if (ticketTypes.Count != ticketTypeIds.Distinct().Count())
            return OrderErrors.InvalidTicketTypes;

        var now = dateTimeProvider.UtcNow;

        // 5. Validate sales windows and reserve capacity
        //    THIS IS THE CRITICAL SECTION
        //    RowVersion on TicketType handles concurrent conflicts at SaveChangesAsync
        var requestedItems = new List<(Guid Id, Guid TicketTypeId, int Quantity, decimal UnitPrice)>();

        foreach (var requestItem in request.Items)
        {
            var ticketType = ticketTypes.First(tt => tt.Id == requestItem.TicketTypeId);

            // Check sales window
            if (!ticketType.IsSalesOpen(now))
                return OrderErrors.TicketSalesNotOpen(ticketType.Name);

            // Reserve capacity — domain method checks available quantity
            var reserveResult = ticketType.Reserve(requestItem.Quantity);
            if (reserveResult.IsError)
                return reserveResult.TopError;

            requestedItems.Add((
                Guid.NewGuid(),
                ticketType.Id,
                requestItem.Quantity,
                ticketType.Price));  // server-side price — never trust the client
        }

        // 6. Create the order
        var orderNumber = GenerateOrderNumber();

        var orderResult = Order.Create(
            Guid.NewGuid(),
            @event.TenantId,
            request.EventId,
            attendeeId,
            orderNumber,
            requestedItems);

        if (orderResult.IsError)
            return orderResult.Errors!;

        var order = orderResult.Value;

        // 7. Persist order and updated TicketType reservations atomically
        //    If two requests reserved the last ticket simultaneously,
        //    SaveChangesAsync throws DbUpdateConcurrencyException here
        //    because TicketType.RowVersion will have changed between our read and write
        await db.Orders.AddAsync(order, ct);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Another request won the race — tell the client to retry
            return OrderErrors.ConcurrencyConflict;
        }

        // 8. Build response — load ticket type names for the DTO
        var items = order.OrderItems
            .Select(item => (
                Item: item,
                TicketTypeName: ticketTypes.First(tt => tt.Id == item.TicketTypeId).Name))
            .ToList();

        return order.ToDto(@event.EventName, items);
    }

    private static string GenerateOrderNumber()
    {
        // EF-{timestamp}-{random} — readable, sortable, unique enough for MVP
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
        var random = RandomNumberGenerator.GetInt32(1000, 9999);
        return $"EF-{timestamp}-{random}";
    }
}
