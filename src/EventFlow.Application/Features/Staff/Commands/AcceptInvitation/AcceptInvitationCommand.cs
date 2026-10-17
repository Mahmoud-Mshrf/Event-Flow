using EventFlow.Domain.Common.Results;
using MediatR;

namespace EventFlow.Application.Features.Staff.Commands.AcceptInvitation;

public sealed record AcceptInvitationCommand(
    string InviteeEmail,
    string Token,
    string Name,
    string Password) : IRequest<Result<Success>>;
