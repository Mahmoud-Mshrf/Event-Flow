using EventFlow.Domain.Common.Errors;
using EventFlow.Domain.Common.Results;
using MediatR;

namespace EventFlow.Application.Features.CheckIn.Commands.CheckInAttendee;
public sealed record CheckInAttendeeCommand(
    string QrCode,
    Guid EventId) : IRequest<Result<CheckInResultDto>>;

public sealed record CheckInResultDto(
    string TicketNumber,
    string AttendeeName,
    string TicketTypeName,
    DateTime CheckedInAt);

public static class CheckInErrors
{
    public static readonly Error Unauthorized =
        Error.Forbidden("CheckIn.Unauthorized",
            "You must be a tenant staff member to perform check-ins.");
}

