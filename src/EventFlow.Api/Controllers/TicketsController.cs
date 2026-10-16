using EventFlow.Application.Common.Contracts;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Tickets.Dtos;
using EventFlow.Application.Features.Tickets.Queries.GetMyTickets;
using EventFlow.Application.Features.Tickets.Queries.GetTicketById;
using EventFlow.Domain.Tickets.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.Api.Controllers;

[Route("api/tickets")]
[Authorize]
public sealed class TicketsController(
    ISender sender,
    ICurrentUser currentUser) : ApiController
{
    [HttpGet("my-tickets")]
    [ProducesResponseType(typeof(PaginatedList<TicketSummaryDto>), StatusCodes.Status200OK)]
    [EndpointSummary("Get all tickets for the current attendee.")]
    [EndpointName("GetMyTickets")]
    public async Task<ActionResult> GetMyTickets(
        [FromQuery] PageRequest page,
        [FromQuery] TicketStatus? status,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetMyTicketsQuery
        {
            AttendeeId = currentUser.UserId,
            Status = status,
            Page = page.Page,
            PageSize = page.PageSize
        }, ct);

        return result.Match(response => Ok(response), Problem);
    }

    [HttpGet("{ticketId:guid}")]
    [ProducesResponseType(typeof(TicketDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get ticket detail with QR code.")]
    [EndpointName("GetTicketById")]
    public async Task<ActionResult> GetById(
        Guid ticketId,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new GetTicketByIdQuery(ticketId, currentUser.UserId), ct);

        return result.Match(response => Ok(response), Problem);
    }
}
