using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Tickets.Dtos;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Tickets;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Tickets.Queries.GetTicketById;

public sealed class GetTicketByIdQueryHandler(
    IAppDbContext db,
    ICurrentUser currentUser)
    : IRequestHandler<GetTicketByIdQuery, Result<TicketDto>>
{
    public async Task<Result<TicketDto>> Handle(
        GetTicketByIdQuery request,
        CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated)
            return TicketErrors.Unauthenticated;

        var ticket = await db.Tickets
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Include(t => t.Event)
            .Include(t => t.TicketType)
            .FirstOrDefaultAsync(t =>
                t.Id == request.TicketId &&
                t.AttendeeId == currentUser.UserId, ct);  // attendee can only see own tickets

        if (ticket is null)
            return TicketErrors.NotFound;

        return new TicketDto(
            ticket.Id,
            ticket.TicketNumber,
            ticket.Event.EventName,
            ticket.Event.Location,
            ticket.Event.StartDate,
            ticket.TicketType.Name,
            ticket.Status,
            ticket.QrCode,
            ticket.CheckedInAt);
    }
}

