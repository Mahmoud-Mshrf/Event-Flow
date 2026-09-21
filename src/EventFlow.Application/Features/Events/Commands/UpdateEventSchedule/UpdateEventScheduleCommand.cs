using EventFlow.Application.Features.Events.Dtos;
using EventFlow.Domain.Common.Results;
using MediatR;

namespace EventFlow.Application.Features.Events.Commands.UpdateEventSchedule;

public sealed record UpdateEventScheduleCommand(
    Guid EventId,
    DateTime StartDate,
    DateTime EndDate) : IRequest<Result<EventDto>>;
