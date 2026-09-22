using EventFlow.Api.Contracts;
using EventFlow.Application.Common.Contracts;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.TicketTypes.commands.CreateTicketType;
using EventFlow.Application.Features.TicketTypes.Commands.DeleteTicketType;
using EventFlow.Application.Features.TicketTypes.Commands.UpdateTicketTypeCapacity;
using EventFlow.Application.Features.TicketTypes.Commands.UpdateTicketTypeName;
using EventFlow.Application.Features.TicketTypes.Commands.UpdateTicketTypePrice;
using EventFlow.Application.Features.TicketTypes.Commands.UpdateTicketTypeSalesWindow;
using EventFlow.Application.Features.TicketTypes.Dtos;
using EventFlow.Application.Features.TicketTypes.Queries.GetTicketTypeById;
using EventFlow.Application.Features.TicketTypes.Queries.GetTicketTypesByEvent;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.Api.Controllers;

[Route("api/events/{eventId:guid}/ticket-types")]
[Authorize]
public sealed class TicketTypesController(
    ISender sender,
    ICurrentTenant currentTenant) : ApiController
{
    [HttpPost]
    [ProducesResponseType(typeof(TicketTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Create a new ticket type for an event.")]
    [EndpointName("CreateTicketType")]
    public async Task<ActionResult> Create(
        Guid eventId,
        [FromBody] CreateTicketTypeRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(new CreateTicketTypeCommand(
            eventId,
            request.Name,
            request.Price,
            request.Capacity,
            request.SalesStart,
            request.SalesEnd), ct);

        return result.Match(response => Ok(response), Problem);
    }

    [HttpPut("{ticketTypeId:guid}/name")]
    [ProducesResponseType(typeof(TicketTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Update ticket type name.")]
    [EndpointName("UpdateTicketTypeName")]
    public async Task<ActionResult> UpdateName(
        Guid ticketTypeId,
        [FromBody] UpdateTicketTypeNameRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateTicketTypeNameCommand(ticketTypeId, request.Name), ct);

        return result.Match(response => Ok(response), Problem);
    }

    [HttpPut("{ticketTypeId:guid}/price")]
    [ProducesResponseType(typeof(TicketTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Update ticket type price.")]
    [EndpointName("UpdateTicketTypePrice")]
    public async Task<ActionResult> UpdatePrice(
        Guid ticketTypeId,
        [FromBody] UpdateTicketTypePriceRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateTicketTypePriceCommand(ticketTypeId, request.Price), ct);

        return result.Match(response => Ok(response), Problem);
    }

    [HttpPut("{ticketTypeId:guid}/capacity")]
    [ProducesResponseType(typeof(TicketTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Update ticket type capacity.")]
    [EndpointName("UpdateTicketTypeCapacity")]
    public async Task<ActionResult> UpdateCapacity(
        Guid ticketTypeId,
        [FromBody] UpdateTicketTypeCapacityRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateTicketTypeCapacityCommand(ticketTypeId, request.Capacity), ct);

        return result.Match(response => Ok(response), Problem);
    }

    [HttpPut("{ticketTypeId:guid}/sales-window")]
    [ProducesResponseType(typeof(TicketTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Update ticket type sales window.")]
    [EndpointName("UpdateTicketTypeSalesWindow")]
    public async Task<ActionResult> UpdateSalesWindow(
        Guid ticketTypeId,
        [FromBody] UpdateTicketTypeSalesWindowRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateTicketTypeSalesWindowCommand(
                ticketTypeId,
                request.SalesStart,
                request.SalesEnd), ct);

        return result.Match(response => Ok(response), Problem);
    }

    [HttpDelete("{ticketTypeId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Delete a ticket type.")]
    [EndpointName("DeleteTicketType")]
    public async Task<ActionResult> Delete(
        Guid ticketTypeId,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new DeleteTicketTypeCommand(ticketTypeId), ct);

        return result.Match(_ => NoContent(), Problem);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<TicketTypeDto>), StatusCodes.Status200OK)]
    [EndpointSummary("Get all ticket types for an event.")]
    [EndpointName("GetTicketTypesByEvent")]
    public async Task<ActionResult> GetByEvent(
        Guid eventId,
        [FromQuery] PageRequest page,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetTicketTypesByEventQuery(
            eventId,
            currentTenant.TenantGuid)
        {
            Page = page.Page,
            PageSize = page.PageSize
        }, ct);

        return result.Match(response => Ok(response), Problem);
    }

    [HttpGet("{ticketTypeId:guid}")]
    [ProducesResponseType(typeof(TicketTypeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a ticket type by id.")]
    [EndpointName("GetTicketTypeById")]
    public async Task<ActionResult> GetById(
        Guid ticketTypeId,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new GetTicketTypeByIdQuery(ticketTypeId, currentTenant.TenantGuid), ct);

        return result.Match(response => Ok(response), Problem);
    }
}