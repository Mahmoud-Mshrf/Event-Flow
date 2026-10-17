using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Users.Enums;
using MediatR;

namespace EventFlow.Application.Features.Staff.Commands.InviteStaff;

public sealed record InviteStaffCommand(
    string InviteeEmail,
    UserRole Role) : IRequest<Result<Success>>;
