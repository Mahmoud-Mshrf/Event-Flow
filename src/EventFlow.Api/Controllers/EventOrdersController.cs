using EventFlow.Application.Common.Contracts;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Orders.Dtos;
using EventFlow.Application.Features.Orders.Queries.GetEventsOrders;
using EventFlow.Domain.Orders.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.Api.Controllers;

[Route("api/events/{eventId:guid}/orders")]
[Authorize(Policy = "TenantStaff")]  // Owner + Employee
public sealed class EventOrdersController(
    ISender sender,
    ICurrentTenant currentTenant) : ApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<OrderSummaryDto>), StatusCodes.Status200OK)]
    [EndpointSummary("Get all orders for an event (organizer view).")]
    [EndpointName("GetEventOrders")]
    public async Task<ActionResult> GetAll(
        Guid eventId,
        [FromQuery] PageRequest page,
        [FromQuery] OrderStatus? status,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetEventOrdersQuery
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