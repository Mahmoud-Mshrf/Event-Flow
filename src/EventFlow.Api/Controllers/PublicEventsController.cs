using EventFlow.Api.Controllers;
using EventFlow.Application.Common.Contracts;
using EventFlow.Application.Features.PublicDiscovery.Dtos;
using EventFlow.Application.Features.PublicDiscovery.Queries.GetPublicEventById;
using EventFlow.Application.Features.PublicDiscovery.Queries.GetPublicEvents;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/public/events")]
[AllowAnonymous]
public sealed class PublicEventsController(ISender sender) : ApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<PublicEventDto>), StatusCodes.Status200OK)]
    [EndpointSummary("Browse public events across all organizers.")]
    [EndpointName("GetPublicEvents")]
    public async Task<ActionResult> GetAll(
        [FromQuery] PageRequest page,
        [FromQuery] string? searchTerm,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetPublicEventsQuery
        {
            Page = page.Page,
            PageSize = page.PageSize,
            SearchTerm = searchTerm,
            FromDate = fromDate,
            ToDate = toDate
        }, ct);

        return result.Match(response => Ok(response), Problem);
    }

    [HttpGet("{eventId:guid}")]
    [ProducesResponseType(typeof(PublicEventDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get public event details including available ticket types.")]
    [EndpointName("GetPublicEventById")]
    public async Task<ActionResult> GetById(
        Guid eventId,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetPublicEventByIdQuery(eventId), ct);
        return result.Match(response => Ok(response), Problem);
    }
}