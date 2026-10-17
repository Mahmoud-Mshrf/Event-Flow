using EventFlow.Api.Contracts;
using EventFlow.Application.Common.Interfaces;
using EventFlow.Application.Features.Staff.Commands.AcceptInvitation;
using EventFlow.Application.Features.Staff.Commands.InviteStaff;
using EventFlow.Application.Features.Staff.Commands.RevokeInvitation;
using EventFlow.Application.Features.Staff.Queries.GetPendingInvitations;
using EventFlow.Application.Features.Staff.Queries.GetStaff;
using EventFlow.Domain.Users.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventFlow.Api.Controllers;

[Route("api/staff")]
public sealed class StaffController(ISender sender,ICurrentTenant currentTenant) : ApiController
{
    // POST /api/staff/invite — Owner only
    [HttpPost("invite")]
    [Authorize(Policy = "TenantOwner")]
    public async Task<IActionResult> Invite(
        [FromBody] InviteStaffRequest request,
        CancellationToken ct)
    {
        var command = new InviteStaffCommand(
            request.InviteeEmail,
            request.Role);

        var result = await sender.Send(command, ct);
        return result.Match(_ => Ok(), Problem);
    }

    // POST /api/staff/accept-invitation — Anonymous
    [HttpPost("accept-invitation")]
    [AllowAnonymous]
    public async Task<IActionResult> AcceptInvitation(
        [FromBody] AcceptInvitationRequest request,
        CancellationToken ct)
    {
        var command = new AcceptInvitationCommand(
            request.InviteeEmail,
            request.Token,
            request.Name,
            request.Password);

        var result = await sender.Send(command, ct);
        return result.Match(_ => Ok(), Problem);
    }

    // DELETE /api/staff/invitations/{invitationId} — Owner only
    [HttpDelete("invitations/{invitationId:guid}")]
    [Authorize(Policy = "TenantOwner")]
    public async Task<IActionResult> RevokeInvitation(
        Guid invitationId,
        CancellationToken ct)
    {
        var command = new RevokeInvitationCommand(invitationId);
        var result = await sender.Send(command, ct);
        return result.Match(_ => NoContent(), Problem);
    }

// GET /api/staff?page=1&pageSize=10 — Owner only
    [HttpGet]
    [Authorize(Policy = "TenantOwner")]
    public async Task<IActionResult> GetStaff(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var query = new GetStaffQuery
        {
            TenantId = currentTenant.TenantGuid,
            Page = page,
            PageSize = pageSize
        };

        var result = await sender.Send(query, ct);
        return result.Match(Ok, Problem);
    }

    // GET /api/staff/invitations?page=1&pageSize=10 — Owner only
    [HttpGet("invitations")]
    [Authorize(Policy = "TenantOwner")]
    public async Task<IActionResult> GetPendingInvitations(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var query = new GetPendingInvitationsQuery
        {
            TenantId = currentTenant.TenantGuid,
            Page = page,
            PageSize = pageSize
        };

        var result = await sender.Send(query, ct);
        return result.Match(Ok, Problem);
    }


}