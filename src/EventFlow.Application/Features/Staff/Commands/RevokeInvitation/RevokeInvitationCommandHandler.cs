using EventFlow.Application.Common.Interfaces;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.StaffInvitations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EventFlow.Application.Features.Staff.Commands.RevokeInvitation;

internal sealed class RevokeInvitationCommandHandler(
    IAppDbContext db)
    : IRequestHandler<RevokeInvitationCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(
        RevokeInvitationCommand request,
        CancellationToken ct)
    {
        // Global filter scopes to current tenant automatically
        var invitation = await db.StaffInvitations
            .FirstOrDefaultAsync(i => i.Id == request.InvitationId, ct);

        if (invitation is null)
            return StaffInvitationErrors.NotFound;

        var result = invitation.Revoke();

        if (result.IsError)
            return result.Errors!;

        await db.SaveChangesAsync(ct);
        return Result.Success;
    }
}