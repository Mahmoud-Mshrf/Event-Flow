using EventFlow.Domain.Common.Results;
using MediatR;

namespace EventFlow.Application.Features.Staff.Commands.RevokeInvitation;

public sealed record RevokeInvitationCommand(Guid InvitationId)
    : IRequest<Result<Success>>;
