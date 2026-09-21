using EventFlow.Application.Features.Events.Dtos;
using EventFlow.Domain.Common.Results;
using EventFlow.Domain.Events.Enums;
using MediatR;

namespace EventFlow.Application.Features.Events.Commands.UpdateEventVisibility;

public sealed record UpdateEventVisibilityCommand(
    Guid EventId,
    EventVisibility Visibility) : IRequest<Result<EventDto>>;
