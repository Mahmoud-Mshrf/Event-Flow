using EventFlow.Api.Contracts;
using EventFlow.Application.Common.Contracts;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Events.Commands.CancelEvent;
using EventFlow.Application.Features.Events.Commands.CreateEvent;
using EventFlow.Application.Features.Events.Commands.PublishCommand;
using EventFlow.Application.Features.Events.Commands.UpdateEventDetails;
using EventFlow.Application.Features.Events.Commands.UpdateEventLocation;
using EventFlow.Application.Features.Events.Commands.UpdateEventRegistrationPeriod;
using EventFlow.Application.Features.Events.Commands.UpdateEventSchedule;
using EventFlow.Application.Features.Events.Commands.UpdateEventVisibility;
using EventFlow.Application.Features.Events.Dtos;
using EventFlow.Application.Features.Events.Queries.GetEventById;
using EventFlow.Application.Features.Events.Queries.GetOrganizerEvents;
using EventFlow.Domain.Events;
using EventFlow.Domain.Events.Enums;
using EventFlow.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.Api.Controllers;

[Route("api/events")]
[Authorize]
public sealed class EventsController(ISender sender,ICurrentTenant currentTenant) : ApiController
{
    [HttpPost]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new event.")]
    [EndpointName("CreateEvent")]
    public async Task<ActionResult> Create(
        [FromBody] CreateEventCommand command,
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.Match(response => Ok(response), Problem);
    }

    [HttpPut("{eventId:guid}/details")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [EndpointSummary("Update event name and description.")]
    [EndpointName("UpdateEventDetails")]
    public async Task<ActionResult> UpdateDetails(
        Guid eventId,
        [FromBody] UpdateEventDetailsRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateEventDetailsCommand(eventId, request.EventName, request.Description), ct);
        return result.Match(response => Ok(response), Problem);
    }

    [HttpPut("{eventId:guid}/location")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Update event location.")]
    [EndpointName("UpdateEventLocation")]
    public async Task<ActionResult> UpdateLocation(
        Guid eventId,
        [FromBody] UpdateEventLocationRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateEventLocationCommand(eventId, request.Location), ct);
        return result.Match(response => Ok(response), Problem);
    }

    [HttpPut("{eventId:guid}/schedule")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Update event start and end dates.")]
    [EndpointName("UpdateEventSchedule")]
    public async Task<ActionResult> UpdateSchedule(
        Guid eventId,
        [FromBody] UpdateEventScheduleRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateEventScheduleCommand(eventId, request.StartDate, request.EndDate), ct);
        return result.Match(response => Ok(response), Problem);
    }

    [HttpPut("{eventId:guid}/registration-period")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Update event registration window.")]
    [EndpointName("UpdateEventRegistrationPeriod")]
    public async Task<ActionResult> UpdateRegistrationPeriod(
        Guid eventId,
        [FromBody] UpdateEventRegistrationPeriodRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateEventRegistrationPeriodCommand(
                eventId, request.RegistrationStart, request.RegistrationEnd), ct);
        return result.Match(response => Ok(response), Problem);
    }

    [HttpPut("{eventId:guid}/visibility")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Update event visibility.")]
    [EndpointName("UpdateEventVisibility")]
    public async Task<ActionResult> UpdateVisibility(
        Guid eventId,
        [FromBody] UpdateEventVisibilityRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new UpdateEventVisibilityCommand(eventId, request.Visibility), ct);
        return result.Match(response => Ok(response), Problem);
    }

    [HttpPost("{eventId:guid}/publish")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Publish an event.")]
    [EndpointName("PublishEvent")]
    public async Task<ActionResult> Publish(
        Guid eventId,
        CancellationToken ct)
    {
        var result = await sender.Send(new PublishEventCommand(eventId), ct);
        return result.Match(response => Ok(response), Problem);
    }

    [HttpPost("{eventId:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Cancel an event.")]
    [EndpointName("CancelEvent")]
    public async Task<ActionResult> Cancel(
        Guid eventId,
        CancellationToken ct)
    {
        var result = await sender.Send(new CancelEventCommand(eventId), ct);
        return result.Match(_ => NoContent(), Problem);
    }

    [HttpGet("{eventId:guid}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get event details by id.")]
    [EndpointName("GetEventById")]
    public async Task<ActionResult> GetById(
        Guid eventId,
        CancellationToken ct)
    {

        var result = await sender.Send(new GetEventByIdQuery(eventId, currentTenant.TenantGuid), ct);
        return result.Match(response => Ok(response), Problem);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<EventDto>), StatusCodes.Status200OK)]
    [EndpointSummary("Get all events for the current tenant.")]
    [EndpointName("GetOrganizerEvents")]
    public async Task<ActionResult> GetAll(
        [FromQuery] PageRequest page,
        [FromQuery] EventStatus? status,
        [FromQuery] EventVisibility? visibility,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new GetOrganizerEventsQuery
        {
            TenantId =currentTenant.TenantGuid,
            Status = status,
            Visibility = visibility,
            Page = page.Page,
            PageSize = page.PageSize
        }, ct);

        return result.Match(response => Ok(response), Problem);
    }
}
