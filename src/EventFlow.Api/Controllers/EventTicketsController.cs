using EventFlow.Application.Common.Contracts;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Tickets.Dtos;
using EventFlow.Application.Features.Tickets.Queries.GetEventTickets;
using EventFlow.Domain.Tickets.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.Api.Controllers;

// Organizer ticket view — nested under events
[Route("api/events/{eventId:guid}/tickets")]
[Authorize(Policy = "TenantStaff")]  // Owner + Employee
public sealed class EventTicketsController(
    ISender sender,
    ICurrentTenant currentTenant) : ApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<TicketSummaryDto>), StatusCodes.Status200OK)]
    [EndpointSummary("Get all tickets for an event (organizer view).")]
    [EndpointName("GetEventTickets")]
    public async Task<ActionResult> GetAll(
        Guid eventId,
        [FromQuery] PageRequest page,
        [FromQuery] TicketStatus? status,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetEventTicketsQuery
        {
            EventId = eventId,
            TenantId = currentTenant.TenantGuid,
            Status = status,
            Page = page.Page,
            PageSize = page.PageSize
        }, ct);

        return result.Match(response => Ok(response), Problem);
    }
}