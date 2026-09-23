using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Orders.Commands.CancelOrder;

public sealed class CancelOrderCommandHandler(
    IAppDbContext db,
    ICurrentUser currentUser)
    : IRequestHandler<CancelOrderCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(
        CancelOrderCommand request,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
            return OrderErrors.Unauthenticated;

        // Load order with items — need items to release capacity
        var order = await db.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct);

        if (order is null)
            return OrderErrors.NotFound;

        // Confirm this order belongs to the caller
        // Global query filter handles tenant scoping for organizer queries
        // But for attendee self-service we check AttendeeId explicitly
        if (order.AttendeeId != currentUser.UserId)
            return OrderErrors.Unauthorized;

        var cancelResult = order.Cancel();
        if (cancelResult.IsError)
            return cancelResult.TopError;

        // Release capacity back to ticket types
        // Order.Cancel() raises OrderCancelledDomainEvent
        // which the existing handler picks up and releases capacity
        // So we only need to SaveChangesAsync here
        await db.SaveChangesAsync(ct);

        return Result.Success;
    }
}