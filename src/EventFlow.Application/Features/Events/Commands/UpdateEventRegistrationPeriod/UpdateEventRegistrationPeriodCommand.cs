using EventFlow.Application.Features.Events.Dtos;
using EventFlow.Domain.Common.Results;
using MediatR;

namespace EventFlow.Application.Features.Events.Commands.UpdateEventRegistrationPeriod;

public sealed record UpdateEventRegistrationPeriodCommand(
    Guid EventId,
    DateTime RegistrationStart,
    DateTime RegistrationEnd) : IRequest<Result<EventDto>>;
