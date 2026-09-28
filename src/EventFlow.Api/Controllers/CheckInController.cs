using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.CheckIn.Commands.CheckInAttendee;
using EventFlow.Application.Features.CheckIn.Queries.GetCheckInStats;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.Api.Controllers;

[Route("api/events/{eventId:guid}/check-in")]
[Authorize(Policy = "TenantStaff")]  // Owner + Employee both can check in
public sealed class CheckInController(
    ISender sender,
    ICurrentTenant currentTenant) : ApiController
{
    [HttpPost]
    [ProducesResponseType(typeof(CheckInResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [EndpointSummary("Check in an attendee by scanning their QR code.")]
    [EndpointName("CheckInAttendee")]
    public async Task<ActionResult> CheckIn(
        Guid eventId,
        [FromBody] CheckInRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new CheckInAttendeeCommand(request.QrCode, eventId), ct);

        return result.Match(response => Ok(response), Problem);
    }

    [HttpGet("stats")]
    [ProducesResponseType(typeof(CheckInStatsDto), StatusCodes.Status200OK)]
    [EndpointSummary("Get real-time check-in statistics for this event.")]
    [EndpointName("GetCheckInStats")]
    public async Task<ActionResult> GetStats(
        Guid eventId,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new GetCheckInStatsQuery(eventId, currentTenant.TenantGuid), ct);

        return result.Match(response => Ok(response), Problem);
    }
}

public sealed record CheckInRequest(string QrCode);